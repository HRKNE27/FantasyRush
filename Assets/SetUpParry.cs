using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetUpParry : MonoBehaviour
{
    [SerializeField] private GameObject parryCounter;
    [SerializeField] private GameObject parryBar;
    [SerializeField] private GameObject pointA;
    [SerializeField] private GameObject pointB;
    public EnemyStats enemyStats;

    private List<int> parryTimeSet;
    private Vector3 lastPosition;
    private Coroutine moveCoroutine;
    [SerializeField] private float distanceTravled = 0f;
    private float elapsedTime = 0f;

    /*private void Start()
    {
        SetParryWindow();
        parryCounter.transform.position = pointA.transform.position;
        lastPosition = pointA.transform.position;
        moveCoroutine = StartCoroutine(MoveParryCounter());
    }*/

    private void OnEnable()
    {
        SetParryWindow();
        parryCounter.transform.position = pointA.transform.position;
        lastPosition = pointA.transform.position;
        moveCoroutine = StartCoroutine(MoveParryCounter());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            CheckSuccess();
    }

    private void SetParryWindow()
    {
        List<int> availibleIndexes = new List<int>();
        List<int> selectedIndexes = new List<int>();
        for (int i = 0; i < parryBar.transform.childCount; i++)
            availibleIndexes.Add(i);
        int tempCount = Mathf.Clamp(enemyStats.ParryWindow, 0, parryBar.transform.childCount);
        while(selectedIndexes.Count < tempCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, availibleIndexes.Count);
            selectedIndexes.Add(availibleIndexes[randomIndex] + 1);
            Transform randomChild = parryBar.transform.GetChild(availibleIndexes[randomIndex]);
            randomChild.GetComponent<SpriteRenderer>().color = Color.red;
            Debug.Log(availibleIndexes[randomIndex]);
            availibleIndexes.RemoveAt(randomIndex);
            
        }
        parryTimeSet = selectedIndexes;
    }

    private void CheckSuccess()
    {
        StopCoroutine(moveCoroutine);
        float parryPercentage = distanceTravled / Vector3.Distance(pointA.transform.position, pointB.transform.position);
        Debug.Log(parryPercentage);
        foreach (int x in parryTimeSet)
        {
            float startPercent = (x - 1) / 10f;
            float endPercent = x / 10f;
            Debug.Log(startPercent.ToString() + " - " + endPercent.ToString());
            if (CheckSuccess(parryPercentage, startPercent, endPercent))
                return;
        }
        moveCoroutine = StartCoroutine(MoveParryCounter());
            
    }

    IEnumerator MoveParryCounter()
    {
        while(elapsedTime < enemyStats.ParryDuration)
        {
            float t = elapsedTime/enemyStats.ParryDuration;
            Vector3 newPosition = Vector3.Lerp(pointA.transform.position, pointB.transform.position, t);

            distanceTravled += Vector3.Distance(parryCounter.transform.position, newPosition);
            parryCounter.transform.position = newPosition;

            lastPosition = transform.position;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        distanceTravled += Vector3.Distance(parryCounter.transform.position, pointB.transform.position);
        parryCounter.transform.position = pointB.transform.position;
        Debug.Log("Launch attack");
    }

    public static bool CheckSuccess(float number, float min, float max)
    {
        Debug.Log(number >= min && number <= max);
        return number >= min && number <= max;
    }
}
