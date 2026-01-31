using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum KeyPressed
{
    Q,
    W,
    E,
    R
}

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    // Movement Input
    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;
    public static bool SwapLeftPressed;
    public static bool SwapRightPressed;
    public static bool QPressed;
    public static bool WPressed;
    public static bool EPressed;
    public static bool RPressed;


    private bool _qPressed;
    private bool _wPressed;
    private bool _ePressed;
    private bool _rPressed;

    public static float _qTimer;
    public static float _wTimer;
    public static float _eTimer;
    public static float _rTimer;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;
    private InputAction _swapLeftAction;
    private InputAction _swapRightAction;
    private InputAction _qAction;
    private InputAction _wAction;
    private InputAction _eAction;
    private InputAction _rAction;



    // Update is called once per frame
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];
        _swapLeftAction = PlayerInput.actions["SwapLeft"];
        _swapRightAction = PlayerInput.actions["SwapRight"];
        _qAction = PlayerInput.actions["Q"];
        _wAction = PlayerInput.actions["W"];
        _eAction = PlayerInput.actions["E"];
        _rAction = PlayerInput.actions["R"];
    }

    private void OnEnable()
    {
        _qAction.Enable();
        _wAction.Enable();
        _eAction.Enable();
        _rAction.Enable();

        _qAction.performed += ctx => { _qPressed = true; _qTimer = 0f; };
        _qAction.canceled += ctx => { _qPressed = false; };

        _wAction.performed += ctx => { _wPressed = true; _wTimer = 0f; };
        _wAction.canceled += ctx => { _wPressed = false; };

        _eAction.performed += ctx => { _ePressed = true; _eTimer = 0f; };
        _eAction.canceled += ctx => { _ePressed = false; };

        _rAction.performed += ctx => { _rPressed = true; _rTimer = 0f; };
        _rAction.canceled += ctx => { _rPressed = false; };
    }

    private void OnDisable()
    {
        _qAction.Disable();
        _wAction.Disable();
        _eAction.Disable();
        _rAction.Disable();
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        // Jump
        JumpWasPressed = _jumpAction.WasPerformedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        // Movement
        RunIsHeld = _runAction.IsPressed();
        DashWasPressed = _dashAction.WasPressedThisFrame();

        // Swap
        SwapLeftPressed = _swapLeftAction.WasPressedThisFrame();
        SwapRightPressed = _swapRightAction.WasPressedThisFrame();

        // TODO: Need to add checks for when each attack button is pressed for charge attacks like archer and wizard
        // Attack
        QPressed = _qAction.WasPressedThisFrame();
        WPressed = _wAction.WasPressedThisFrame();
        EPressed = _eAction.WasPressedThisFrame();
        RPressed = _rAction.WasPressedThisFrame();

        UpdateQTimer();
        UpdateWTimer();
        UpdateETimer();
        UpdateRTimer();
    }

    public static void ResetTimer(KeyPressed key)
    {
        switch (key)
        {
            case KeyPressed.Q:
                _qTimer = 0;
                break;
            case KeyPressed.W:
                _wTimer = 0;
                break;
            case KeyPressed.E:
                _eTimer = 0;
                break;
            case KeyPressed.R:
                _rTimer = 0;
                break;
        }
    }

    private void UpdateQTimer()
    {
        if (_qPressed)
        {
            _qTimer += Time.deltaTime;
        }
        else
        {
            _qTimer = 0f;
        }
    }

    private void UpdateWTimer()
    {
        if (_wPressed)
        {
            _wTimer += Time.deltaTime;
        }
        else
        {
            _wTimer = 0f;
        }
    }

    private void UpdateETimer()
    {
        if (_ePressed)
        {
            _eTimer += Time.deltaTime;
        }
        else
        {
            _eTimer = 0f;
        }
    }

    private void UpdateRTimer()
    {
        if (_rPressed)
        {
            _rTimer += Time.deltaTime;
        }
        else
        {
            _rTimer = 0f;
        }
    }
}
