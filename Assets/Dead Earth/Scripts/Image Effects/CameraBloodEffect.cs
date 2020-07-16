using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraBloodEffect : MonoBehaviour {

    [SerializeField]
    private Texture2D _bloodTexture;
    [SerializeField]
    private Texture2D _bloodNormalMap;
    [SerializeField]
    private float _bloodAmount = 0.0f;
    [SerializeField]
    private float _minBloodAmount = 0.0f;
    [SerializeField]
    private float _distortion = 1.0f;
    [SerializeField]
    private bool _autoFade = true;
    [SerializeField]
    private float _fadeSpeed = 0.05f;

    [SerializeField]
    private Shader _shader;

    private Material _material;

    public float BloodAmount { get => _bloodAmount; set => _bloodAmount = value; }
    public float MinBloodeAmount { get => _minBloodAmount; set => _minBloodAmount = value; }
    public float FadeSpeed { get => _fadeSpeed; set => _fadeSpeed = value; }
    public bool AutoFade { get => _autoFade; set => _autoFade = value; }

    public void Update() {
        if (_autoFade) {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (_shader == null)
            return;
        if (_material == null) {
            _material = new Material(_shader);
        }

        if (_material == null)
            return;

        if (_bloodTexture != null)
            _material.SetTexture("_BloodTex", _bloodTexture);
        if (_bloodNormalMap != null)
            _material.SetTexture("_BloodBump", _bloodNormalMap);

        _material.SetFloat("_Distortion", _distortion);
        _material.SetFloat("_BloodAmount", _bloodAmount);

        Graphics.Blit(source, destination, _material);
    }
}
