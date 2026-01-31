using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdSpell : WizardBaseState
{
    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);

        _attackIndex = 3;
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
