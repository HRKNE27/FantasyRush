using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField] private float globalLightShakeForce = 0.25f;
    [SerializeField] private float globalMediumShakeForce = 0.5f;
    [SerializeField] private float globalHeavyShakeForce = 0.75f;
    [SerializeField] private CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;
    public static GeneralShakeIntensity shakeIntensity;

    public enum GeneralShakeIntensity
    {
        Light,
        Medium,
        Heavy
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CameraShake(CinemachineImpulseSource impulseSource, GeneralShakeIntensity generalShakeIntensity = GeneralShakeIntensity.Medium)
    {
        switch (generalShakeIntensity)
        {
            case GeneralShakeIntensity.Light:
                impulseSource.GenerateImpulseWithForce(globalLightShakeForce);
                break;
            case GeneralShakeIntensity.Medium:
                impulseSource.GenerateImpulseWithForce(globalLightShakeForce);
                break;
            case GeneralShakeIntensity.Heavy:
                impulseSource.GenerateImpulseWithForce(globalLightShakeForce);
                break;
            default:
                impulseSource.GenerateImpulseWithForce(globalMediumShakeForce);
                break;
        }
        
    }

    public void ScreenShakeFromProfile(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        SetUpScreenShakeSettings(profile, impulseSource);
        impulseSource.GenerateImpulseWithForce(profile.impactForce);
    }

    private void SetUpScreenShakeSettings(ScreenShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.m_ImpulseDefinition;

        // Change Impulse Source Settings
        impulseDefinition.m_ImpulseDuration = profile.impactTime;
        impulseSource.m_DefaultVelocity = profile.defaultVelocity;
        impulseDefinition.m_CustomImpulseShape = profile.impulseCurve;

        // Change Impulse Listener Settings
        impulseListener.m_ReactionSettings.m_AmplitudeGain = profile.listenerAmplitude;
        impulseListener.m_ReactionSettings.m_FrequencyGain = profile.listenerFrequency;
        impulseListener.m_ReactionSettings.m_Duration = profile.listenerDuration;
    }

    
}
