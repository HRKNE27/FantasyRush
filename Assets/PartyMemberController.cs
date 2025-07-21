using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyMemberController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ScreenShakeProfile screenShakeProfile;
    [SerializeField] private ParticleSystem damageParticles;

    private Animator _anim;
    private PlayerMovement _playerMovement;
    private Dissolve _dissolve;
    private CinemachineImpulseSource _impulseSource;
    private ParticleSystem damageParticlesInstance;

    [SerializeField] private int currentHealth;
    private bool isAlive;
    private bool iFrame;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _dissolve = GetComponent<Dissolve>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        currentHealth = playerStats.Health;
        isAlive = true;
        iFrame = false;
    }

    void Update()
    {
        CheckHealth();
    }

    public void TakeDamage(int incomingDamage, Vector2 attackDirection)
    {
        if (!iFrame)
        {
            currentHealth -= incomingDamage;
            SpawnDamageParticles(attackDirection);
            // CameraShakeManager.Instance.CameraShake(_impulseSource);
            CameraShakeManager.Instance.ScreenShakeFromProfile(screenShakeProfile, _impulseSource);
            InitiateInvincibility(playerStats.InvincibleTimeFrame);
        }
        
    }

    public void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            _playerMovement.StopAllCoroutines();
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

    public void InitiateInvincibility(float time)
    {
        StartCoroutine(InvincibilityCooldown(time));
    }

    private IEnumerator InvincibilityCooldown(float timeFrame)
    {
        iFrame = true;
        Debug.Log("sadjlaskjlksajlkjsalkdslka");
        yield return new WaitForSeconds(timeFrame);
        iFrame = false;
    }

    public void SetInvincibility(bool isInvincible)
    {
        iFrame = isInvincible;
    }

    private IEnumerator DeathSequence()
    {
        _anim.StopPlayback();
        _anim.Play("Death");
        _playerMovement._canAct = false;
        Invoke("DestroyPlayer", 1f);
        yield break;
    }

    private void DestroyPlayer()
    {
        if (isAlive == false)
            _dissolve.CallVanishPlayer(gameObject);
    }
}
