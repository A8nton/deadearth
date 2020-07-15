using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBloodEffect : MonoBehaviour {

    [SerializeField]
    private Shader _shader;

    private Material _material;


    public void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (_shader == null)
            return;
        if (_material == null) {
            _material = new Material(_shader);
        }

        if (_material == null)
            return;

        Graphics.Blit(source, destination, _material);
    }
}
