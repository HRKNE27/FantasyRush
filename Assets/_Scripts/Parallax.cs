using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera _camera;
    public Transform _player;
    Vector2 _startPosition;
    float _startZ, _starting, _bgLength;


    Vector2 _travel => (Vector2) _camera.transform.position - _startPosition;
    float _distanceFromSubject => transform.position.z - _player.position.z;
    float _clippingPlane => (_camera.transform.position.z + (_distanceFromSubject > 0 ? _camera.farClipPlane : _camera.nearClipPlane));
    float _parallaxFactor => Mathf.Abs(_distanceFromSubject) / _clippingPlane;

    public void Start()
    {
        _startPosition = transform.position;
        _starting = transform.position.x;
        _startZ = transform.position.z;
        _bgLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    public void Update()
    {
        float temp = (_camera.transform.position.x * (1 - _parallaxFactor));
        Vector2 newPos = _startPosition + _travel * _parallaxFactor;
        transform.position = new Vector3(newPos.x, transform.position.y, _startZ);
    }
}
