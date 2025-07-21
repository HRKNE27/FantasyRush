using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

public class EnemyAI : Monster
{
    #region VARIABLES
    [Header("Pathfinding")]
    [SerializeField] private Transform target;
    [SerializeField] private float aggroRange = 10f;
    [SerializeField] private float disengageRange = 20f;
    [SerializeField] private float pathUpdateSec = 0.5f;
    private bool isAggroed;

    [Header("Physics")]
    [SerializeField] private float speed = 200f;
    [SerializeField] private float acceleration = 0.5f;
    [SerializeField] private float nextWaypoint = 3f;
    [SerializeField] private float jumpNodeHeightReq = 0.8f;
    [SerializeField] private float jumpHeight = 0.3f;
    [SerializeField] private float jumpCheckOffset = 0.1f;

    [Header("Behavior")]
    [SerializeField] private Transform attackArea;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackDelay = .5f;
    [SerializeField] private bool followEnabled = true;
    [SerializeField] private bool jumpEnabled = true;
    private bool canAttack = true;
    private bool isAttacking;
    private bool ignorePlayer;
    public bool canAct;

    [Header("Combat")]
    [SerializeField] private Collider2D _hitCollider;
    private List<Collider2D> _collidersDamaged;

    [Header("Misc")]
    [SerializeField] private GameObject parryCanvas;

    [Space(10)]
    private Path path;
    private int currentWaypoint = 0;
    private bool isJumping, isInAir, onCooldown;
    private Animator _anim;
    Seeker seeker;
    #endregion

    protected override void Start()
    {
        base.Start();
        seeker = GetComponent<Seeker>();
        _anim = GetComponent<Animator>();
        _collidersDamaged = new List<Collider2D>();

        movingRight = true;
        isJumping = false;
        isInAir = false;
        onCooldown = false;
        canAct = true;

        InvokeRepeating("UpdatePath", 0f, pathUpdateSec);
    }

    private void FixedUpdate()
    {
        if (!canAttack) return;

        if (canAct)
        {
            if (TargetInAttackDistance())
            {
                parryCanvas.SetActive(true);
            }
            else if (TargetInDistance() && followEnabled && canAttack)
            {
                PathFollow();
            }
            else
            {
                Move();
            }
        }
    }

    public IEnumerator AttackSequence()
    {     
        canAttack = false;
        float attackNumber = Random.Range(1, enemyStats.AttackNumber+1);
        _anim.SetTrigger("Attack" + attackNumber);

        isAttacking = true;
        Attack();

        yield return new WaitForSeconds(enemyStats.AttackCooldown);
        if (TargetInAttackDistance()) _anim.Play("Idle");
        else _anim.Play("Walk");

        canAttack = true;
        parryCanvas.SetActive(false);
        isAttacking = false;

        // yield return null;
    }

    public void Attack() => StartCoroutine(RegisterAttack());
    public void StopAttack() => isAttacking = false;
    private IEnumerator RegisterAttack()
    {
        while (isAttacking)
        {
            Collider2D[] collidersToDamage = new Collider2D[10];
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            int colliderCount = Physics2D.OverlapCollider(_hitCollider, filter, collidersToDamage);
            for (int i = 0; i < colliderCount; i++)
            {
                
                if (collidersToDamage[i].transform.parent.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Debug.Log(FindParent(collidersToDamage[i])?.name);
                    if (movingRight)
                    {
                        FindParent(collidersToDamage[i]).GetComponent<PartyMemberController>().TakeDamage(enemyStats.Damage,Vector2.right);
                    }
                    else
                    {
                        FindParent(collidersToDamage[i]).GetComponent<PartyMemberController>().TakeDamage(enemyStats.Damage,Vector2.left);
                    }
                    // FindParent(collidersToDamage[i]).GetComponent<PartyMemberController>().TakeDamage();
                    yield break;
                }
            }

            yield return null;
        }

    }

    private GameObject FindParent(Collider2D collider2D)
    {
        return collider2D.transform.parent.gameObject.transform.parent.gameObject;
    }

    private void Move()
    {
        if (!GroundAhead() || WallAhead())
        {
            TurnAround();
        }

        float moveDir = movingRight ? 1 : -1;
        rb.velocity = new Vector2(moveDir * enemyStats.MaxWalkSpeed, rb.velocity.y);
        _anim.SetFloat("MoveSpeed", Mathf.Abs(rb.velocity.x));
    }

    private bool GroundAhead() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    private bool WallAhead() => Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);
    private bool IsGrounded() => Physics2D.OverlapCircle(transform.position, 0.2f, groundLayer);

    private void UpdatePath()
    {
        if (followEnabled && TargetInDistance() && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    private void PathFollow()
    {
        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed;
        Vector2 currentVelocity = rb.velocity;

        Jump(direction);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypoint) currentWaypoint++;

        if (rb.velocity.x > 0f && !movingRight) TurnAround();
        else if (rb.velocity.x < 0f && movingRight) TurnAround();

        rb.velocity = Vector2.SmoothDamp(rb.velocity, force, ref currentVelocity, acceleration);
        
        

    }

    private void Jump(Vector2 direction)
    {
        if (jumpEnabled && IsGrounded() && !isInAir && !onCooldown)
        {
            if (direction.y > jumpNodeHeightReq)
            {
                isJumping = true;
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                StartCoroutine(JumpCoolDown());
            }
        }

        if (IsGrounded())
        {
            isJumping = false;
            isInAir = false;
        }
        else
        {
            isInAir = true;
        }
    }

    public override void MonsterDie()
    {
        base.MonsterDie();
    }

    private bool TargetInDistance()
    {
        if (isAggroed)
        {
            return isAggroed = Vector2.Distance(transform.position, target.transform.position) < aggroRange;
        }
        else
        {
            return isAggroed = Vector2.Distance(transform.position, target.transform.position) < disengageRange;
        }
    }

    private bool TargetInAttackDistance()
    {
        return Vector2.Distance(transform.position, target.transform.position) < nextWaypoint;
    }

    private void FacePlayer()
    {
        if (target.position.x < transform.position.x && movingRight) TurnAround();
        else if (target.position.x > transform.position.x && !movingRight) TurnAround();
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    IEnumerator JumpCoolDown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1f);
        onCooldown = false;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (isAttacking)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackArea.position, attackRadius);
        }

        Color newColor = Color.yellow;
        newColor.a = 0.1f;

        Gizmos.color = newColor;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Color newColor2 = Color.red;
        newColor2.a = 0.1f;

        Gizmos.color = newColor2;
        Gizmos.DrawWireSphere(transform.position, disengageRange);
    }
}
