using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    public void ChangeTarget(Transform newTarget)
    {
        foreach(Transform child in transform)
        {
            child.GetComponent<EnemyAI>().SetTarget(newTarget);        
        }
    }
}
