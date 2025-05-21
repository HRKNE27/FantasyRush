using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    }
}
