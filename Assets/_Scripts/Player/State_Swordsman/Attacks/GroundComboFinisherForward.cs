using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundComboFinisherForward : MeleeBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        _attackIndex = 5;
        _duration = 0.5f;
        _attackDamage = 25;
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
            _playerMovement.IsGroundAttacking(false);
            stateMachine.SetNextStateToMain();
        }
    }
}
