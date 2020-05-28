using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private Animator _animator;
    private int _horizontalHash;
    private int _verticalHash;
    private int _attackHash;

    void Start() {
        _animator = GetComponent<Animator>();
        _horizontalHash = Animator.StringToHash("Horizontal");
        _verticalHash = Animator.StringToHash("Vertical");
        _attackHash = Animator.StringToHash("Attack");
    }

    void Update() {
        float x = Input.GetAxis("Horizontal") * 2.32f;
        float y = Input.GetAxis("Vertical") * 5.66f;

        if (Input.GetMouseButtonDown(0)) {
            _animator.SetTrigger(_attackHash);
        }
        _animator.SetFloat(_horizontalHash, x, 0.1f, Time.deltaTime);
        _animator.SetFloat(_verticalHash, y, 1, Time.deltaTime);
    }
}
