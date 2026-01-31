using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardBaseState : State
{
    // How long this state should be active for
    public float _duration;

    // Cached Animator component
    protected Animator _animator;

    // Check whether or not the next attack in the sequence should be played or not
    protected bool _shouldCombo;

    //The attack index in the sequence of attacks
    protected int _attackIndex;

    protected PlayerMovement _playerMovement;
    private float _attackPressedTimer = 0;


    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        _attackPressedTimer -= Time.deltaTime;
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
