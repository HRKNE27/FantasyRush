using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartParry : MonoBehaviour
{
    [SerializeField] private GameObject parryCanvas;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            parryCanvas.SetActive(true);
    }
}
