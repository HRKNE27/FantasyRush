using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundComboFollowState : MeleeBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        _attackIndex = 3;
        _duration = 0.286f;
        _attackDamage = 70;
        _animator.SetTrigger("Attack" + _attackIndex);
        _playerMovement.IsGroundAttacking(true);
        Debug.Log("Player attack " + _attackIndex + " launched");
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= _duration)
        {
            if (_shouldCombo && InputManager.Movement.y == 1.0f)
            {
                stateMachine.SetNextState(new GroundComboFinisherUp());
            }
            else if(_shouldCombo && Mathf.Abs(InputManager.Movement.x)  == 1.0f)
            {
                stateMachine.SetNextState(new GroundComboFinisherForward());
            }
            else if (_shouldCombo && InputManager.Movement.y == -1.0f) 
            {
                stateMachine.SetNextState(new GroundComboFinisherDown());
            }
            else
            {
                _playerMovement.IsGroundAttacking(false);
                stateMachine.SetNextStateToMain();
            }
        }
    }
}
