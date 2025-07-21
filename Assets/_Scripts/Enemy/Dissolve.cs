using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [SerializeField] private float _dissolveTime;

    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private EnemyAI _enemyAI;

    private bool _deathCheck;
    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = GetComponent<Renderer>().material;
        // _enemyAI = GetComponent<EnemyAI>();
        _deathCheck = true;
    }

    public void CallVanish(GameObject gameObject)
    {
        if (_deathCheck)
        {
            StartCoroutine(Vanish(gameObject));
            _deathCheck = false;
        }
        
    }

    public void CallVanishPlayer(GameObject gameObject)
    {
        if (_deathCheck)
        {
            StartCoroutine(VanishPlayer(gameObject));
            _deathCheck = false;
        }

    }

    public void CallAppear()
    {
        StartCoroutine(Appear());
    }

    private IEnumerator Vanish(GameObject g)
    {
        float elapsedTime = 0f;
        while(elapsedTime <= _dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(1f, 0f, (elapsedTime / _dissolveTime));
            _material.SetFloat(_dissolveAmount, lerpedDissolve);
            yield return null;
        }
        if (elapsedTime > _dissolveTime)
            Destroy(g);
        yield return null;
    }

    private IEnumerator VanishPlayer(GameObject g)
    {
        float elapsedTime = 0f;
        while (elapsedTime <= _dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(1f, 0f, (elapsedTime / _dissolveTime));
            _material.SetFloat(_dissolveAmount, lerpedDissolve);
            yield return null;
        }
        if (elapsedTime > _dissolveTime)
            PartyController.Instance.LeaderDeathUpdate();
        yield return null;
    }

    private IEnumerator Appear()
    {
        float elapsedTime = 0f;
        while (elapsedTime <= _dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(0f, 1f, (elapsedTime / _dissolveTime));
            _material.SetFloat(_dissolveAmount, lerpedDissolve);
            yield return null;
        }
    }
}
