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

    private Animator _anim;
    private EnemyAI _enemyAI;
    private Dissolve _dissolve;
    private CinemachineImpulseSource _impulseSource;
    private ParticleSystem damageParticlesInstance;

    private int currentHealth;
    private int currentBarrier;
    private bool alive;

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
        alive = true;
    }

    void Update()
    {
        CheckBarrier();
        CheckHealth();
    }

    public void TakeDamage(int incomingDamage, Vector2 attackDirection)
    {
        int damageTaken = (int) Mathf.Floor((float)incomingDamage * enemyStats.BarrierDamageReduction);
        if(currentBarrier > 0)
        {
            currentHealth -= damageTaken;
            currentBarrier -= damageTaken;
            // Debug.Log(transform.gameObject.name + " has taken " + damageTaken.ToString());
        }
        else
        {
            currentHealth -= incomingDamage;
            // Debug.Log(transform.gameObject.name + " has taken " + incomingDamage.ToString());
        }
        SpawnDamageParticles(attackDirection);
        CameraShakeManager.Instance.CameraShake(_impulseSource);
        CameraShakeManager.Instance.ScreenShakeFromProfile(screenShakeProfile, _impulseSource);
    }

    public void CheckBarrier()
    {
        if (currentBarrier <= 0)
        {
            StartCoroutine(StunCooldown());         
        }
    }

    public void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            _enemyAI.StopAllCoroutines();
            StartCoroutine(DeathSequence());
            alive = false;
        }
    }

    private void SpawnDamageParticles(Vector2 attackDirection)
    {
        Quaternion spawnRotation = Quaternion.FromToRotation(Vector2.right, attackDirection);
        damageParticlesInstance = Instantiate(damageParticles, transform.position, spawnRotation);
    }

    private IEnumerator StunCooldown()
    {
        _enemyAI.canAct = false;
        _anim.SetBool("Stunned",true);
        yield return new WaitForSeconds(enemyStats.StunDuration);
        currentBarrier = enemyStats.BarrierThreshold;
        _anim.SetBool("Stunned", false);
        _enemyAI.canAct = true;
        _anim.Play("Walk");
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
        if(alive == false)
            _dissolve.CallVanish();
    }
}
