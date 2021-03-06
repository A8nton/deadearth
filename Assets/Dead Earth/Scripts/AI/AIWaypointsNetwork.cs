﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathDisplayMode { None, Conections, Paths }

public class AIWaypointsNetwork : MonoBehaviour {

    [HideInInspector]
    public PathDisplayMode DisplayMode = PathDisplayMode.Conections;
    [HideInInspector]
    public int UiStart = 0;
    [HideInInspector]
    public int UiEnd = 0;
    public List<Transform> Waypoints = new List<Transform>();
}
