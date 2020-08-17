using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The PlayerInput register all the inputs related to the player and communicate with the other player scripts.
public class PlayerInput : MonoBehaviour
{

    public bool pickUpItem
    {
        get { return Input.GetKeyDown(KeyCode.E); }
    }

    //____________MovementInputs_____________\\

    public Vector2 input
    {
        get
        {
            Vector2 movementVector = Vector2.zero;

            movementVector.x = Input.GetAxis("Horizontal");
            movementVector.y = Input.GetAxis("Vertical");

            movementVector *= (movementVector.x != 0.0f && movementVector.y != 0.0f) ? .7071f : 1.0f;

            return movementVector;
        }
    }

    public Vector2 down
    {
        get { return _down; }
    }

    public Vector2 raw
    {
        get
        {
            Vector2 rawVector = Vector2.zero;
            rawVector.x = Input.GetAxisRaw("Horizontal");
            rawVector.y = Input.GetAxisRaw("Vertical");
            rawVector *= (rawVector.x != 0.0f && rawVector.y != 0.0f) ? .7071f : 1.0f;
            return rawVector;
        }
    }

    public bool crouch
    {
        get { return Input.GetKeyDown(KeyCode.C); }
    }

    public bool crouching
    {
        get { return Input.GetKey(KeyCode.C); }
    }

    public bool slide
    {
        get { return Input.GetKeyDown(KeyCode.LeftShift); }
    }

    //____________DefaultsInputs_____________\\

    public bool flashLight
    {
        get { return Input.GetKeyDown(KeyCode.F); }
    }

    public bool useMedkit
    {
        get { return Input.GetKeyDown(KeyCode.Q); }
    }

    public bool pause 
    { get { return Input.GetKeyDown(KeyCode.Escape); } }

    //____________GeneralInputs_____________\\

    private Vector2 _previous;
    private Vector2 _down;

    private int  _jumpTimer;
    private bool _jump;

    
    void Start()
    {
        _jumpTimer = -1;
    }

    void Update()
    {
        _down = Vector2.zero;


        if (raw.x != _previous.x)
        {
            _previous.x = raw.x;

            if (_previous.x != 0)
                _down.x = _previous.x;
        }
        if (raw.y != _previous.y)
        {
            _previous.y = raw.y;

            if (_previous.y != 0)
                _down.y = _previous.y;
        }
    }

    private void FixedUpdate()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            _jump = false;
            _jumpTimer++;
        }
        else if (_jumpTimer > 0)
        {
            _jump = true;
        }
    }

    //____________JumpInputs_____________\\

    public bool Jump()
    {
        return _jump;
    }

    public void ResetJump()
    {
        _jumpTimer = -1;
    }

    
}
