using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LocationManagerEnums;

namespace GoShared
{
    [Serializable]
    public class GOMotionStateEvent : UnityEvent<MotionState>
    {


    }

    [Serializable]
    public class GOLocationEvent : UnityEvent<Coordinates>
    {


    }
}
