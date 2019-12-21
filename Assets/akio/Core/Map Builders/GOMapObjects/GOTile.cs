using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using MiniJSON;
using UnityEngine.AI;
using GoShared;

namespace GoMap
{
    [ExecuteInEditMode]
    public abstract class GOTile : MonoBehaviour
    {

        public GOTileObj goTile;

        [HideInInspector] public GOMap map;

        ParseJob job;
        IList buildingsIds = new List<object>();

        public abstract IEnumerator BuildTile(IDictionary mapData, GOLayer layer, bool delayedLoad);
        public abstract IEnumerator ParseTileData(GOLayer[] layers, bool delayedLoad, List<string> layerNames);
        public abstract string vectorUrl();
        public abstract string demUrl();
        public abstract string normalsUrl();
        public abstract string satelliteUrl(Vector2? tileCoords = null);

        #region Terrain

        public void createTileBackground()
        {

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            filter.sharedMesh = goTile.groundMesh();
            gameObject.AddComponent<MeshCollider>();
            renderer.material = map.tileBackground;

        }

        public void AddTerrainToLayerMask(string name, GameObject obj)
        {
            LayerMask mask = LayerMask.NameToLayer(name);
            if (mask.value > 0 && mask.value < 31)
            {
                obj.layer = LayerMask.NameToLayer(name);
            }
            else
            {
                Debug.LogWarning("[GOMap] Please create a Unity Layer named GOTerrain to use maps with elevation");
            }
        }

        #endregion

        #region BUILDINGS

        private float mdc = 60; // Group buildings every 50meters
        public GOCenterContainer findNearestCenter(Vector3 center, GameObject parent)
        {

            foreach (GOCenterContainer c in map.containers)
            {
                float d = Vector3.Distance(center, c.center);
                if (d <= mdc)
                {
                    return c;
                }
            }

            if (map.containers.Count > 100)
                map.containers.Clear();

            GOCenterContainer container = new GOCenterContainer();
            container.center = center;
            map.containers.Add(container);
            return container;
        }

        public Material GetMaterial(GORenderingOptions rendering, Vector3 center)
        {

            if (rendering.materials.Length > 0)
            {
                float seed = center.x * center.z * 100;

                seed = Mathf.Max(1, seed);

                System.Random rnd = new System.Random((int)seed);
                int pick = rnd.Next(0, rendering.materials.Length);
                Material material = rendering.materials[pick];
                return material;
            }
            else
                return rendering.material;
        }

        void OnDestroy()
        {
            removeIds();
            Resources.UnloadUnusedAssets();
            //			Debug.Log ("Destroy tile: "+gameObject.name);
        }

        public bool idCheck(object id, GOLayer layer)
        {
            if (map.buildingsIds.Contains(id))
            {
                //				Debug.Log ("id already created");
                return false;
            }
            else
            {
                //					Debug.Log ("id added");
                buildingsIds.Add(id);
                map.buildingsIds.Add(id);
                return true;
            }
        }

        private void removeIds()
        {
            foreach (object id in buildingsIds)
            {
                map.buildingsIds.Remove(id);
            }
        }

        #endregion


        public void AddObjectToLayerMask(GOLayer layer, GameObject obj)
        {
            LayerMask mask = LayerMask.NameToLayer(layer.name);
            if (mask.value > 0 && mask.value < 31)
            {
                obj.layer = LayerMask.NameToLayer(layer.name);
            }
            else
            {
                Debug.LogWarning("[GOMap] Please create layer masks before running GoMap. A layer mask must have the same name declared in GoMap inspector, for example \"" + layer.name + "\".");
            }
        }

        #region Loaders

        public void PrepareGoTile()
        {

            if (map != null)
            {
                goTile.useStreetnames = !map.labels.disabled;
                goTile.streetNames = map.labels.streetNames;
            }

            if (goTile.useElevation)
            {

                if (!string.IsNullOrEmpty(demUrl()))
                {
                    GOTileData demData = new GOTileData(demUrl(), goTile, GOTileData.GODataType.DEM);
                    goTile.tileData.Add(demData);
                }
                if (!string.IsNullOrEmpty(normalsUrl()))
                {
                    GOTileData normalsData = new GOTileData(normalsUrl(), goTile, GOTileData.GODataType.Normals);
                    goTile.tileData.Add(normalsData);
                }
            }

            if (goTile.useSatelliteBackground && !string.IsNullOrEmpty(satelliteUrl()))
            {

                if (goTile.satellite4X)
                {

                    foreach (Vector2 tileC in goTile.tileCenter.subTiles(goTile.tileCoordinates))
                    {
                        GOTileData satelliteData = new GOTileData(satelliteUrl(tileC), goTile, GOTileData.GODataType.Satellite4X, tileC);
                        satelliteData.tileCoords = tileC;
                        goTile.tileData.Add(satelliteData);
                    }
                }
                else
                {
                    GOTileData satelliteData = new GOTileData(satelliteUrl(), goTile, GOTileData.GODataType.Satellite);
                    goTile.tileData.Add(satelliteData);
                }
            }

            if (goTile.navmeshSettings.buildNavmeshSurfaces)
                 AddNavmesh();

            GOTileData vectorData = new GOTileData(vectorUrl(), goTile, GOTileData.GODataType.VectorPBF);
            goTile.tileData.Add(vectorData);
        }

        public IEnumerator LoadTileData(GOLayer[] layers, bool delayedLoad)
        {

            goTile.SetupAdiacentTiles(map.tiles);
            PrepareGoTile();
            BuildFeaturePrototype();

            //if (map.useElevation)
            AddTerrainToLayerMask("GOTerrain", gameObject);

            //			goTile.ShowDebugMarkers ();

            if (Application.isPlaying)
            {
                yield return StartCoroutine(goTile.downloadData(this));
                if (map.useSatelliteBackground)
                {
                    MeshRenderer renderer = GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    renderer.sharedMaterial = material;
                    renderer.sharedMaterial.mainTexture = goTile.getSatelliteTexture();
                }
                yield return StartCoroutine(ParseTileData(layers, delayedLoad, map.layerNames()));

            }
            else
            {
                goTile.downloadData(this, () =>
                {
                    if (map.useSatelliteBackground)
                    {
                        MeshRenderer renderer = GetComponent<MeshRenderer>();
                        Material material = Material.Instantiate(renderer.sharedMaterial);
                        material.mainTexture = goTile.getSatelliteTexture();
                        renderer.sharedMaterial = material;
                    }
                    GORoutine.start(ParseTileData(layers, delayedLoad, map.layerNames()), this);

                });
            }
        }

        public IEnumerator ParseJson(string data)
        {

            job = new ParseJob();
            job.InData = data;
            job.Start();

            yield return StartCoroutine(job.WaitFor());
        }

        public void OnTileLoaded () {

            if (goTile.navmeshSettings.buildNavmeshSurfaces)
                BuildNavmesh();
        }

        #endregion


        #region Utils

        public void BuildFeaturePrototype()
        {

            GameObject prefab = new GameObject("Feature prototype");
            prefab.transform.SetParent(map.transform);
            prefab.AddComponent<MeshFilter>();
            prefab.AddComponent<MeshRenderer>();
            //			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            prefab.AddComponent<MeshCollider>().enabled = false;

            goTile.featurePrototype = prefab;

            GameObject streetname = new GameObject("Street prototype");
            streetname.transform.SetParent(map.transform);
            streetname.AddComponent<MeshRenderer>();
            streetname.AddComponent<TextMesh>();
            streetname.AddComponent<GOStreetName>();
            goTile.streetnamePrototype = streetname;

        }

        #endregion

        #region Navemesh

        public void AddNavmesh () {

            NavMeshSurface navmeshSurface = gameObject.AddComponent<NavMeshSurface>();
            navmeshSurface.agentTypeID = goTile.navmeshSettings.AgentTypeIndex;
            navmeshSurface.collectObjects = goTile.navmeshSettings.CollectObjects;
            navmeshSurface.layerMask = goTile.navmeshSettings.IncludeLayers;
            navmeshSurface.useGeometry = goTile.navmeshSettings.m_UseGeometry;

            if (goTile.navmeshSettings.useNavmeshLinks)
                gameObject.AddComponent<NavMeshLink>();
        }

        public void BuildNavmesh () {
            
            NavMeshSurface navmeshSurface = gameObject.GetComponent<NavMeshSurface>();
            navmeshSurface.BuildNavMesh();
        }

        #endregion
    }
}
