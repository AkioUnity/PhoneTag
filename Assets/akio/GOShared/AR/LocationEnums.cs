using System.Collections;
using System.Collections.Generic;
using GoShared;
using UnityEngine;

namespace LocationManagerEnums
{

    public enum DemoLocation
    {
        NewYork,
        Rome,
        NewYork2,
        Venice,
        SanFrancisco,
        Berlin,
        RioDeJaneiro,
        Paris,
        Budapest,
        GrandCanyon,
        Matterhorn,
        London,
        SearchMode,
        NoGPSTest,
        Custom
    };

    public enum MotionPreset
    {
        Run,
        Bike,
        Car
    };

    public enum MotionMode
    {
        Avatar,
        GPS,
        UnityRemote
    };

    public enum MotionState
    {
        Idle,
        Walk,
        Run
    };

    public static class LocationEnums
    {
        
        #region DEMO LOCATIONS

        public static Coordinates GetCoordinates(DemoLocation demoLocation)
        {

            switch (demoLocation)
            {
                case DemoLocation.NewYork:
                   return new Coordinates(40.783435, -73.966249, 0);
                case DemoLocation.NewYork2:
                    return new Coordinates(40.70193632375534, -74.01628977185595, 0);
                case DemoLocation.Rome:
                    return new Coordinates(41.910509366663945, 12.476284503936768, 0);
                case DemoLocation.Venice:
                    return new Coordinates(45.433184, 12.336831, 0);
                case DemoLocation.SanFrancisco:
                    return new Coordinates(37.8019180297852, -122.419631958008, 0);
                case DemoLocation.Berlin:
                    return new Coordinates(52.521123, 13.409396, 0);
                case DemoLocation.RioDeJaneiro:
                    return new Coordinates(-22.9638023376465, -43.1685562133789, 0);
                //case DemoLocation.Dubai:
                    //return new Coordinates (25.197469, 55.274366,0);
                case DemoLocation.Budapest:
                    return new Coordinates(47.50261987827267, 19.039907455444336, 0);
                case DemoLocation.Paris:
                    return new Coordinates(48.873769, 2.294745, 0);
                case DemoLocation.GrandCanyon:
                    return  new Coordinates(36.0979385375977, -112.066040039063, 0);
                case DemoLocation.Matterhorn:
                    return new Coordinates(45.976574, 7.6562632, 0);
                case DemoLocation.London:
                    return new Coordinates(51.5129522, -0.0982975, 0);
                case DemoLocation.NoGPSTest:
                case DemoLocation.SearchMode:
                case DemoLocation.Custom:
                    return null;
                default:
                    return null;
            }

        }

        #endregion

    }
}