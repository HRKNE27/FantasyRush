using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboCharacter : MonoBehaviour
{
    private StateMachine _meleeStateMachine;
    private PlayerMovement _playerMovement;

    [SerializeField] public Collider2D _hitbox;
    // [SerializeField] public GameObject _hitEffect;

    // Start is called before the first frame update
    void Start()
    {
        _meleeStateMachine = GetComponent<StateMachine>();
        _playerMovement = _meleeStateMachine._playerMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.QPressed && _meleeStateMachine.CurrentState.GetType() == typeof(IdleCombatState) )
        {
            if (_playerMovement._isGrounded)
            {
                _playerMovement.IsGroundAttacking(true);
                _meleeStateMachine.SetNextState(new GroundEntryState());
            }else if (!_playerMovement._isGrounded && InputManager.Movement.y == 1.0f)
            {
                _playerMovement.IsAirAttacking(true);
                _meleeStateMachine.SetNextState(new AerialSlashUp());
            }else if(!_playerMovement._isGrounded && InputManager.Movement.y == -1.0f)
            {
                _playerMovement.IsAirAttacking(true);
                _meleeStateMachine.SetNextState(new AerialSlashDown());
            }
            else if(!_playerMovement._isGrounded && Mathf.Abs(InputManager.Movement.x) == 1.0f)
            {
                _playerMovement.IsAirAttacking(true);
                _meleeStateMachine.SetNextState(new AerialSlashForward());
            }
        }

        if(InputManager.WPressed && _meleeStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            if (_playerMovement._isGrounded)
            {
                _playerMovement.IsGroundAttacking(true);
                _meleeStateMachine.SetNextState(new GroundWhirl());
            }
            else if(!_playerMovement._isGrounded)
            {
                _playerMovement.IsAirAttacking(true);
                _meleeStateMachine.SetNextState(new AerialWhirl());
            }
        }

        if(InputManager.EPressed && _meleeStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            if (_playerMovement._isGrounded)
            {
                _playerMovement.IsGroundAttacking(true);
                _meleeStateMachine.SetNextState(new Parry());
            }
        }

        if (InputManager.RPressed && _meleeStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            if (_playerMovement._isGrounded)
            {
                _playerMovement.IsGroundAttacking(true);
                _meleeStateMachine.SetNextState(new HeavySlash());
            }
        }
    }
}
