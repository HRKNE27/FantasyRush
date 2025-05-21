using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Move Stats")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 5f;
    [Range(1f, 100f)] public float MaxRunSpeed = 10f;
    [Range(0f, 1f)] public float Acceleration = 0.5f;
    [Range(1f, 100f)] public float JumpForce = 5f;


    [Header("Pathfininding")]
    [Range(1f, 50f)] public float DetectionRange = 10f;
    [Range(1f, 50f)] public float DisengageRange = 15f;

    [Header("Health & Defence Stats")]
    [Range(1, 1000)] public int Health = 50;
    [Range(0f, 1f)] public float BarrierDamageReduction = 0.6f;     // How much damage reduction if barrier is up
    [Range(1, 1000)] public int BarrierThreshold = 25;              // How strong the barrier is
    [Range(1f, 100f)] public float StunDuration = 5f;
    [Range(1f, 100f)] public float DeathDuration = 5f;

    [Header("Attack Stats")]
    [Range(1, 100)] public int Damage = 10;
    [Range(1, 10)] public int AttackNumber = 3;                     // How many different attack animations does the oponent have
    [Range(0f, 1f)] public float CriticalChance = 0.01f;
    [Range(1f, 5f)] public float CriticalDamage = 1f;
    [Range(1f, 10f)] public float AttackCooldown = 3f;
    [Range(0f, 10f)] public float AttackDelay = 0.5f;
    [Range(0f, 10f)] public float AttackRadius = 1f;

    [Header("Parry Stats")]
    // How fast is parry counter
    // How many open window frames to get parried
    [Range(1, 100)] public float ParryDuration = 1;
    [Range(0, 10)] public int ParryWindow = 2; 
}
