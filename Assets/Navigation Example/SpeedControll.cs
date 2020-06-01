using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class SpeedControll : MonoBehaviour {

    [SerializeField]
    private float _speed;
    private Animator _controller;

    void Start() {
        _controller = GetComponent<Animator>();
    }

    void Update() {
        _controller.SetFloat("Speed", _speed);
    }
}
