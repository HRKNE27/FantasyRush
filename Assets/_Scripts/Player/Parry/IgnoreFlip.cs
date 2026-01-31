using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreFlip : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        Vector3 parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(
            parentScale.x < 0 ? -originalScale.x : originalScale.x,
            originalScale.y,
            originalScale.z
        );
    }
}
