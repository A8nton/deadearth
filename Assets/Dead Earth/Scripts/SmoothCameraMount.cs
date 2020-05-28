using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraMount : MonoBehaviour {

    [SerializeField]
    private Transform _mount;
    [SerializeField]
    private float _speed = 5;

    void Start() {

    }

    void LateUpdate() {
        transform.position = Vector3.Lerp(transform.position, _mount.position, Time.deltaTime * _speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _mount.rotation, Time.deltaTime * _speed);
    }
}