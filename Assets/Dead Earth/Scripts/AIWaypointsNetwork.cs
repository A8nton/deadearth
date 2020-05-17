using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathDisplayMode { None, Conections, Paths }

public class AIWaypointsNetwork : MonoBehaviour {

    public PathDisplayMode DisplayMode = PathDisplayMode.Conections;
    public int UiStart = 0;
    public int UiEnd = 0;
    public List<Transform> Waypoints = new List<Transform>();
}
