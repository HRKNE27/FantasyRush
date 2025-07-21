using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager _instance;
    private List<EnemyAI> _enemyAIList = new List<EnemyAI>();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        foreach (Transform child in transform)
        {
            _enemyAIList.Add(child.GetComponent<EnemyAI>());
        }
    }



    public void ChangeTarget(Transform newTarget)
    {
        foreach(EnemyAI child in _enemyAIList)
        {
            child.SetTarget(newTarget);        
        }
    }
}
