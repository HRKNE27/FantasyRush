using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondSpell : WizardBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        _attackIndex = 2;
        _duration = 0.65f;
        _animator.SetTrigger("Attack" + _attackIndex);
        Debug.Log("Player attack " + _attackIndex + " launched");
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        base.OnUpdate();

        if (fixedtime >= _duration)
        {
            stateMachine.SetNextStateToMain();
        }
    }
}
