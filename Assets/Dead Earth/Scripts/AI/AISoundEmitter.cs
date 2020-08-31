using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour {

    [SerializeField]
    private float _decayRate = 1.0f;

    private SphereCollider _collider;
    private float _srcRadius = 0.0f;
    private float _tgtRadius = 0.0f;
    private float _interpolator = 0.0f;
    private float _interpolatorSpeed = 0.0f;

    void Start() {
        _collider = GetComponent<SphereCollider>();
        if (!_collider) return;

        _srcRadius = _tgtRadius = _collider.radius;

        _interpolator = 0.0f;
        if (_decayRate > 0.02f)
            _interpolatorSpeed = 1.0f / _decayRate;
        else
            _interpolatorSpeed = 0.0f;
    }

    public void FixedUpdate() {
        if (!_collider) return;

        _interpolator = Mathf.Clamp01(_interpolator + Time.deltaTime * _interpolatorSpeed);

        _collider.radius = Mathf.Lerp(_srcRadius, _tgtRadius, _interpolator);

        if (_collider.radius < Mathf.Epsilon)
            _collider.enabled = false;
        else
            _collider.enabled = true;
    }

    public void SetRadius(float newRadius, bool instantSize = false) {
        if (!_collider || newRadius == _tgtRadius) return;

        _srcRadius = (instantSize || newRadius > _collider.radius) ? newRadius : _collider.radius;
        _tgtRadius = newRadius;
        _interpolator = 0.0f;
    }

    public void Update() {
        SetRadius(2.0f);
        if (Input.GetKeyDown(KeyCode.R))
            SetRadius(15.0f);
    }
}
