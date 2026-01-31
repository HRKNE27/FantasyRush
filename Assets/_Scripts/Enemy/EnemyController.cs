using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Cinemachine;

public class EnemyController : MonoBehaviour
{
    // Control Enemy Health, Barrier, Damage Reduction, and Death

    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private ScreenShakeProfile screenShakeProfile;
    [SerializeField] private ParticleSystem damageParticles;
    [SerializeField] private SetUpParry setUpParry;

    private Animator _anim;
    private EnemyAI _enemyAI;
    private Dissolve _dissolve;
    private CinemachineImpulseSource _impulseSource;
    private ParticleSystem damageParticlesInstance;

    private int currentHealth;
    private int currentBarrier;
    private bool isAlive;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();
        _dissolve = GetComponent<Dissolve>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Start()
    {
        currentHealth = enemyStats.Health;
        currentBarrier = enemyStats.BarrierThreshold;
        isAlive = true;
    }

    void Update()
    {
        CheckBarrier();
        CheckHealth();
    }

    public void TakeDamage(int incomingDamage, Vector2 attackDirection)
    {
        int damageTaken = (int) Mathf.Floor((float)incomingDamage * enemyStats.BarrierDamageReduction);
        bool parrySuccess = setUpParry.CheckSuccess();
        if (parrySuccess)
        {
            _enemyAI.CallKnockback(attackDirection, Vector2.up);
        }
        if (currentBarrier <= 0 || parrySuccess)
        {
            currentHealth -= incomingDamage;
        }
        else
        {
            currentHealth -= damageTaken;
            currentBarrier -= damageTaken;
        }
        if (parrySuccess)
            StartCoroutine(StunCooldown(1.0f));
        SpawnDamageParticles(attackDirection);     
        CameraShakeManager.Instance.ScreenShakeFromProfile(screenShakeProfile, _impulseSource);
    }

    public void CheckBarrier()
    {
        if (currentBarrier <= 0)
        {
            StartCoroutine(StunCooldown(enemyStats.StunDuration));
            StartCoroutine(BarrierReset());
        }
    }

    public void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            _enemyAI.StopAllCoroutines();
            StartCoroutine(DeathSequence());
            isAlive = false;
        }
    }

    private void SpawnDamageParticles(Vector2 attackDirection)
    {
        Quaternion spawnRotation;
        if (attackDirection.x > 0)
        {
            spawnRotation = Quaternion.FromToRotation(Vector2.left, -attackDirection);
        }
        else
        {
            spawnRotation = Quaternion.FromToRotation(Vector2.right, attackDirection);
        }
        
        damageParticlesInstance = Instantiate(damageParticles, transform.position, spawnRotation);
    }

    private IEnumerator StunCooldown(float duration)
    {
        _enemyAI.canAct = false;
        _anim.SetBool("Stunned",true);
        yield return new WaitForSeconds(duration);
        _anim.SetBool("Stunned", false);
        _enemyAI.canAct = true;
        _anim.Play("Walk");
    }

    private IEnumerator BarrierReset()
    {
        yield return new WaitForSeconds(enemyStats.StunDuration);
        currentBarrier = enemyStats.BarrierThreshold;
    }

    private IEnumerator DeathSequence()
    {
        _anim.StopPlayback();
        _anim.Play("Death");
        _enemyAI.canAct = false;
        _anim.SetBool("Stunned", false);
        Invoke("DestroyEnemy", 1f);
        yield break; 
    }

    private void DestroyEnemy()
    {
        if(isAlive == false)
            _dissolve.CallVanish(gameObject);
    }
}
