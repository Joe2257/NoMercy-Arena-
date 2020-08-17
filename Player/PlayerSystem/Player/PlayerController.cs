using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The PlayerController manage all the main feature of the player like: movement, headbob and flashlight. 

//HeadBob functions Class.
public enum CurveControlledBobCallbackType { Horizontal, Vertical }

public delegate void CurveControlledBobCallback();

[System.Serializable]
public class CurveControlledBobEvent
{
    public float Time = 0.0f;
    public CurveControlledBobCallback Function = null;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

[System.Serializable]
public class CurveControlledBob
{
    [SerializeField]
    AnimationCurve _bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                                    new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                                    new Keyframe(2f, 0f));

    [SerializeField] float _horizontalMultiplier         = 0.01f;
    [SerializeField] float _verticalMultiplier           = 0.02f;
    [SerializeField] float _verticalHorizontalSpeedRatio = 2.0f;
    [SerializeField] public float _baseInterval          = 1f;

    private float _prevXPlayHead;
    private float _prevYPlayHead;
    private float _xPlayHead;
    private float _yPlayHead;
    private float _curveEndTime;
    private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();

    public void Initialize()
    {
        _curveEndTime = _bobCurve[_bobCurve.length - 1].time;
        _xPlayHead = 0.0f;
        _yPlayHead = 0.0f;
        _prevXPlayHead = 0.0f;
        _prevYPlayHead = 0.0f;
    }

    public void RegisterEventCallback(float Time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.Time = Time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;
        _events.Add(ccbeEvent);
        _events.Sort(
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
            {
                return (t1.Time.CompareTo(t2.Time));
            }
        );
    }

    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval) * _verticalHorizontalSpeedRatio;


        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;

        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;

        for (int i = 0; i < _events.Count; i++)
        {
            CurveControlledBobEvent ev = _events[i];
            if (ev != null)
            {
                if (ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if ((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                        (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                          (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0f);
    }
}


public enum PlayerStatus { Idle, Moving, Crouched, Sliding, ClimbLadder, WallRun, GrabLedge, ClimbLedge, WallJump }
public class PlayerController : MonoBehaviour
{
    public PlayerStatus _playerStatus;
    [Space]
    [SerializeField] private float _slideCoolDown = 3.0f;
    [SerializeField] private float _crouchLerp = 1.5f;
    [SerializeField] private float _runStepLengthen = 1f;
    [SerializeField] private CurveControlledBob _headBob = new CurveControlledBob();

    float _rayDistance;
    float _radius;
    float _height;
    float _halfRadius;
    float _halfHeight;
    float _baseIntervalCache;
    float _halfInterval;

    public GameObject flashLight;

    Vector3 _localSpaceCameraPos = Vector3.zero;

    private bool _isFlashLightOn;

    //Audio
    private AudioSource _audioSource;
    public AudioClip[] _footSteps;
    public AudioClip   _jumpClip;

    //Components
    private CameraController _cameraController;
    private PlayerMovement   _playerMovement;
    private AnimateLean      _animateLean;
    private PlayerInput      _playerInput;
    private PlayerSystem     _playerSystem;
    private Camera           _camera;



    void Start()
    {
        _audioSource      = GetComponent<AudioSource>();
        _playerInput      = GetComponent<PlayerInput>();
        _playerSystem     = GetComponent<PlayerSystem>();
        _playerMovement   = GetComponent<PlayerMovement>();
        _cameraController = GetComponentInChildren<CameraController>();

        InitializePlayerController();
    }

    private void InitializePlayerController()
    {
        _headBob.Initialize();
        _headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);
        _camera = Camera.main;
        _baseIntervalCache = _headBob._baseInterval;
        _halfInterval = _headBob._baseInterval / 2;
        _localSpaceCameraPos = _camera.transform.localPosition;

        _playerMovement._charController = GetComponent<CharacterController>();


        if (_playerMovement._charController != null)
        {
            _slideLimit = _playerMovement._charController.slopeLimit - .1f;
            _radius = _playerMovement._charController.radius;
            _height = _playerMovement._charController.height;
        }

        _halfRadius = _radius / 2;
        _halfHeight = _height / 2;
        _rayDistance = _halfHeight + _radius + 0.1f;
        _isFlashLightOn = false;
        flashLight.SetActive(false);
    }

    //____________Updates_____________\\

    void FixedUpdate()
    {
        DefaultMovement();
    }

    void Update()
    {
        UpdateMovingStatus();
 
        CheckCrouching();
        FlashLight();
        UseMedkit();

        _cameraController.UpdateCamera();
    }

    void UpdateMovingStatus()
    {
        if ((int)_playerStatus <= 1)
        {
            _playerStatus = PlayerStatus.Idle;
            if (_playerInput.input.magnitude > 0.02f)
                _playerStatus = PlayerStatus.Moving;
        }
    }

    //____________DefaultPlayerFunctions_____________\\

    void PlayHeadBob()
    {
        Vector3 speedXZ = new Vector3(_playerMovement._charController.velocity.x, 0.0f, _playerMovement._charController.velocity.z);
        if (speedXZ.magnitude > 0.01f)
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(speedXZ.magnitude * (_playerStatus == PlayerStatus.Crouched || _playerStatus == PlayerStatus.Moving ? 1.0f : _runStepLengthen));
        else
            _camera.transform.localPosition = _localSpaceCameraPos;

        if (_playerStatus == PlayerStatus.WallRun)
            _headBob._baseInterval = _halfInterval;
        else
            _headBob._baseInterval = _baseIntervalCache;
    }

    void FlashLight()
    {
        if (_playerInput.flashLight && !_isFlashLightOn)
        {
            flashLight.SetActive(true);
            _isFlashLightOn = true;
        }
        else if (_playerInput.flashLight && _isFlashLightOn)
        {
            flashLight.SetActive(false);
            _isFlashLightOn = false;
        }
    }

    private void UseMedkit()
    {
        if (_playerInput.useMedkit && _playerSystem._healthPoints < 100f)
        {
            for (int i = 0; i < _playerSystem._medkits.Length; i++)
            {
                if (_playerSystem._medkits[i] > 0)
                {
                    _playerSystem._healthPoints += _playerSystem._medkits[i];

                    _playerSystem._medkits[i] = 0;
                    _playerSystem._medKitsUsage--;

                    break;
                }
            }
        }
    }

    //____________Audio_____________\\

    void PlayFootStepSound()
    {
        if (_playerMovement.grounded)
        {
            _audioSource.PlayOneShot(_footSteps[Random.Range(0, _footSteps.Length)]);
        }
    }

    //____________Movement_____________\\

    void DefaultMovement()
    {
        _playerMovement.Move(_playerInput.input, (_playerStatus == PlayerStatus.Crouched));

        if (_playerMovement.grounded && _playerInput.Jump())
        {
            if (_playerStatus == PlayerStatus.Crouched)
                Uncrouch();
        
            _playerMovement.Jump(Vector3.up, 1f);
            _playerInput.ResetJump();

            _audioSource.PlayOneShot(_jumpClip);
        }

        PlayHeadBob();
        _headBob._baseInterval = _baseIntervalCache;
    }

   
   //Crouch\\
   void CheckCrouching()
   {
       if (!_playerMovement.grounded || (int)_playerStatus > 2) return;
   
       if (_playerInput.crouch)
       {
           if (_playerStatus != PlayerStatus.Crouched)
               Crouch();
           else
               Uncrouch();
       }
   }
   
   private void Crouch()
   {
       _playerMovement._charController.height = _halfHeight;
       _playerStatus = PlayerStatus.Crouched;
   }
   
   void Uncrouch()
   {
       _playerMovement._charController.height = _height;
       _playerStatus = PlayerStatus.Moving;
   }
}
