using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEntryState : MeleeBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
            _attackIndex = 1;
            _duration = 0.429f;
            _attackDamage = 50;
            _animator.SetTrigger("Attack" + _attackIndex);
            _playerMovement.IsGroundAttacking(false);
            Debug.Log("Player attack " + _attackIndex + " launched"); 
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= _duration)
        {
            if (_shouldCombo)
            {
                stateMachine.SetNextState(new GroundComboState());
            }
            else
            {
                _playerMovement.IsGroundAttacking(false);
                stateMachine.SetNextStateToMain();
            }
        }
    }
}
