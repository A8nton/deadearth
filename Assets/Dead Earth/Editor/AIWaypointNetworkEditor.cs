using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[CustomEditor(typeof(AIWaypointsNetwork))]
public class AIWaypointNetworkEditor : Editor {

    void OnSceneGUI() {
        AIWaypointsNetwork network = (AIWaypointsNetwork)target;
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.normal.textColor = Color.white;

        for (int i = 0; i < network.Waypoints.Count; i++) {
            Transform transform = network.Waypoints[i];

            if (transform != null) {
                Handles.Label(transform.position, "Waypoint " + i.ToString(), guiStyle);
            }
        }

        if (network.DisplayMode == PathDisplayMode.Conections) {
            DrawConnections(network);
        } else if (network.DisplayMode == PathDisplayMode.Paths) {
            DrawPaths(network);
        }
    }


    private static void DrawConnections(AIWaypointsNetwork network) {
        Vector3[] linePoins = new Vector3[network.Waypoints.Count + 1];

        for (int i = 0; i <= network.Waypoints.Count; i++) {
            int index = i != network.Waypoints.Count ? i : 0;

            if (network.Waypoints[index] != null) {
                linePoins[i] = network.Waypoints[index].position;
            } else {
                linePoins[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
        }
        Handles.DrawPolyLine(linePoins);
    }

    private static void DrawPaths(AIWaypointsNetwork network) {
        NavMeshPath path = new NavMeshPath();
        Vector3 from = network.Waypoints[network.UiStart].position;
        Vector3 to = network.Waypoints[network.UiEnd].position;

        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
        Handles.color = Color.yellow;
        Handles.DrawPolyLine(path.corners);
    }
}