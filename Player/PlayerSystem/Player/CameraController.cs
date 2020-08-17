using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Simple camera controller.
public class CameraController : MonoBehaviour
{

    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public GameObject _characterBody;

    [SerializeField] private Vector2 _clampInDegrees = new Vector2(360, 180);
    [SerializeField] private Vector2 _sensitivity = new Vector2(2, 2);
    [SerializeField] private Vector2 _smoothing = new Vector2(3, 3);
    [SerializeField] private Vector2 _targetDirection;
    [SerializeField] private Vector2 _targetCharacterDirection;

    public bool _isCameraLocked = false;


    
    void Start()
    {
        _targetDirection = transform.localRotation.eulerAngles;

        if (_characterBody)
            _targetCharacterDirection = _characterBody.transform.localRotation.eulerAngles;
    }

    public void UpdateCamera()
    {
        if (!_isCameraLocked)
        {
            Quaternion targetOrientation = Quaternion.Euler(_targetDirection);
            Quaternion targetCharacterOrientation = Quaternion.Euler(_targetCharacterDirection);

            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(_sensitivity.x * _smoothing.x, _sensitivity.y * _smoothing.y));


            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / _smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / _smoothing.y);

            _mouseAbsolute += _smoothMouse;

            if (_clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -_clampInDegrees.x * 0.5f, _clampInDegrees.x * 0.5f);


            if (_clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -_clampInDegrees.y * 0.5f, _clampInDegrees.y * 0.5f);

            transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;

            if (_characterBody)
            {
                Quaternion yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
                _characterBody.transform.localRotation = yRotation * targetCharacterOrientation;
            }
            else
            {
                Quaternion yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
                transform.localRotation *= yRotation;
            }
        }
        
    }
}
