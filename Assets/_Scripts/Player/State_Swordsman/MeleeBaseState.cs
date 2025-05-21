using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeBaseState : State
{
    // How long this state should be active for
    public float _duration;

    // Cached Animator component
    protected Animator _animator;

    // Check whether or not the next attack in the sequence should be played or not
    protected bool _shouldCombo;

    //The attack index in the sequence of attacks
    protected int _attackIndex;

    protected int _attackDamage;

    protected PlayerMovement _playerMovement;
    protected Collider2D _hitCollider;
    private List<Collider2D> _collidersDamaged;
    // private GameObject _hitEffectPrefab;
    private float _attackPressedTimer = 0;


    public override void OnEnter(StateMachine stateMachine)
    {
        base.OnEnter(stateMachine);
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();

        _collidersDamaged = new List<Collider2D>();
        _hitCollider = GetComponent<ComboCharacter>()._hitbox;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        _attackPressedTimer -= Time.deltaTime;

        if (_animator.GetFloat("Weapon.Active") > 0f)
            Attack();

        if (InputManager.QPressed)
            _attackPressedTimer = 2;

        if (InputManager.QPressed && _animator.GetFloat("AttackWindow.Open") > 0 && _attackPressedTimer > 0)
            _shouldCombo = true;
    }

    protected void Attack()
    {
        Collider2D[] collidersToDamage = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int colliderCount = Physics2D.OverlapCollider(_hitCollider, filter, collidersToDamage);
        for(int i = 0; i < colliderCount; i++)
        {
            if (!_collidersDamaged.Contains(collidersToDamage[i]))
            {
                TeamComponent hitTeamComponent = collidersToDamage[i].GetComponentInChildren<TeamComponent>();
                EnemyController enemyController = collidersToDamage[i].GetComponentInChildren<EnemyController>();
                if (hitTeamComponent && hitTeamComponent.teamIndex == TeamIndex.Enemy)
                {
                    // Debug.Log("Enemy has taken: " + _attackIndex + "damage");
                    if (_playerMovement._isFacingRight)
                    {
                        enemyController.TakeDamage(_attackDamage, Vector2.right);
                    }
                    else
                    {
                        enemyController.TakeDamage(_attackDamage, Vector2.left);
                    }
                    
                    _collidersDamaged.Add(collidersToDamage[i]);
                }
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }

}
