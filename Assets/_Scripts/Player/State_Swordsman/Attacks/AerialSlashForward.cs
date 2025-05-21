using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialSlashForward : MeleeBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        _attackIndex = 9;
        _duration = 0.625f;
        _animator.SetTrigger("Attack" + _attackIndex);
        _playerMovement.IsAirAttacking(true);
        Debug.Log("Player attack " + _attackIndex + " launched");
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= _duration)
        {
            _playerMovement.IsAirAttacking(false);
            stateMachine.SetNextStateToMain();
        }
    }
}
