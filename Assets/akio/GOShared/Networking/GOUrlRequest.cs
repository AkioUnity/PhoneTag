using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace GoShared {

	public static class GOUrlRequest {

		public static bool verboseLogging = true;

		public static IEnumerator testRequest (string url,Action done) {

			var www = new WWW( url);
			yield return www;

			if(www.error == null) {
				Debug.Log("Request Success: " + url);
				done ();
			}else{
				Debug.LogWarning ("Request Failed: " + url + " :" + www.error);
				done ();
			}
		}

		public static IEnumerator getRequest(MonoBehaviour host, string url, bool useCache ,string filename ,Action <byte[],string,string> response)
		{

			if (Application.isPlaying) { //Runtime build

				if (useCache && FileHandler.Exist(filename))
				{
					byte[] file = FileHandler.Load (filename);
					response(file,null,null);
				}
				else
				{
					var www = new WWW(url);
					yield return www;
					if (string.IsNullOrEmpty(www.error) && www.bytes.Length > 0) {
						Debug.Log ("[GOUrlRequest]  " + url);
						if (useCache)
							FileHandler.Save (filename, www.bytes);
					}else if (www.error != null && (www.error.Contains("429") || www.error.Contains("timed out"))) {
						Debug.LogWarning("[GOUrlRequest] data reload "+www.error + " " + url);
						yield return new WaitForSeconds(1);
						yield return host.StartCoroutine (getRequest(host,url,useCache,filename,response));
						yield break;
					}else {
						Debug.LogWarning("[GOUrlRequest] Tile data missing "+www.error + " " + url);
						response(null,null,www.error);
						yield break;
					}

//					byte[] image = www.bytes;
					response (www.bytes,www.text,www.error);
				}

			} 
			else { //Editor build

				if (useCache && FileHandler.Exist(filename))
				{
					byte[] file = FileHandler.Load (filename);
					response (file,null,null);
				}
				else
				{
			#if UNITY_EDITOR
					var www = new WWW(url);

					ContinuationManager.Add(() => www.isDone, () => {
						
						if (string.IsNullOrEmpty(www.error) && www.bytes.Length > 0) {
							Debug.Log ("[GOUrlRequest]  " + url);
							if (useCache)
								FileHandler.Save (filename, www.bytes);
							response(www.bytes,www.text,null);
						}
						else if (www.error != null && (www.error.Contains("429") || www.error.Contains("timed out"))) {
							Debug.LogWarning("[GOUrlRequest] data reload "+www.error);
							System.Threading.Thread.Sleep(1000);
							GORoutine.start(getRequest(host,url,useCache,filename,response),host);
						}
						else {
							Debug.LogWarning("[GOUrlRequest] data missing "+www.error + " url: "+url);
							response(null,null,www.error);
						}
					});
			#endif
					yield break;
				}
			}

			yield return null;
		}


		public static IEnumerator jsonRequest(MonoBehaviour host, string url, bool useCache ,string filename ,Action <Dictionary<string,object>,string> response)
		{

			ParseJob job = new ParseJob();

			if (Application.isPlaying) { //Runtime build

				if (useCache && FileHandler.Exist(filename))
				{

					job.InData = FileHandler.LoadText (filename);
					job.Start();
					yield return host.StartCoroutine(job.WaitFor());
					response((Dictionary<string,object>)job.OutData,null);
				}
				else
				{
					var www = new WWW(url);
					yield return www;
					if (string.IsNullOrEmpty(www.error) && www.bytes.Length > 0) {
						Debug.Log ("[GOUrlRequest]  " + url);
						if (useCache)
							FileHandler.Save (filename, www.bytes);
					}else if (www.error != null && (www.error.Contains("429") || www.error.Contains("timed out"))) {
						Debug.LogWarning("[GOUrlRequest] data reload "+www.error);
						yield return new WaitForSeconds(1);
						yield return host.StartCoroutine (jsonRequest(host,url,useCache,filename,response));
						yield break;
					}else {
						Debug.LogWarning("[GOUrlRequest] data missing "+www.error+" "+url);
						response(null,www.error);
						yield break;
					}


					job.InData = www.text; //FileHandler.LoadText (filename);
					job.Start();
					yield return host.StartCoroutine(job.WaitFor());
					response((Dictionary<string,object>)job.OutData,null);

				}

			} 
			else { //Editor build

				if (useCache && FileHandler.Exist(filename))
				{
					response((Dictionary<string,object>)Json.Deserialize (FileHandler.LoadText (filename)),null);
				}
				else
				{
			#if UNITY_EDITOR
					var www = new WWW(url);

					ContinuationManager.Add(() => www.isDone, () => {

						if (String.IsNullOrEmpty(www.error) && www.bytes.Length > 0) {
							Debug.Log ("[GOUrlRequest]  " + url);
							if(useCache)
								FileHandler.Save (filename, www.bytes);
							response((Dictionary<string,object>)Json.Deserialize (
								FileHandler.LoadText (filename)),null);
						}
						else if (!String.IsNullOrEmpty(www.error) && (www.error.Contains("429") || www.error.Contains("timed out"))) {
							Debug.LogWarning("[GOUrlRequest] data reload "+www.error);
							System.Threading.Thread.Sleep(1000);
							GORoutine.start(jsonRequest(host,url,useCache,filename,response),host);
						}
						else {
							Debug.LogWarning("[GOUrlRequest] Tile data missing "+www.error);
							response(null,www.error);
						}
					});
			#endif
					yield break;
				}
			}
			yield return null;
		}
	}
}