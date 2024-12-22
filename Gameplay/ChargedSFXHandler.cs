using Unity.FPS.Game;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ChargedSFXHandler : MonoBehaviour
{
    [Tooltip("Audio clip for charge SFX")]
    public AudioClip ChargeSound;

    [Tooltip("Sound played in loop after the change is full for this weapon")]
    public AudioClip LoopChargeWeaponSfx;

    [Tooltip("Duration of the cross fade between the charge and the loop sound")]
    public float FadeLoopDuration = 0.5f;

    [Tooltip("If true, the ChargeSound will be ignored and the pitch on the " +
             "LoopSound will be procedural, based on the charge amount")]
    public bool UseProceduralPitchOnLoopSfx;

    [Range(1.0f, 5.0f), Tooltip("Maximum procedural Pitch value")]
    public float MaxProceduralPitchValue = 2.0f;

    WeaponController m_WeaponController;

    AudioSource m_AudioSource;
    AudioSource m_AudioSourceLoop;

    float m_LastChargeTriggerTimestamp;
    float m_ChargeRatio;
    float m_EndchargeTime;


    void Awake()
    {
        // find references
        m_WeaponController = GetComponent<WeaponController>();
        DebugUtility.HandleErrorIfNullGetComponent<WeaponController, ChargedSFXHandler>(
            m_WeaponController, this, gameObject);

        m_LastChargeTriggerTimestamp = 0.0f;

        // The charge effect needs it's own AudioSources, since it will play on top of the other gun sounds
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        m_AudioSource.clip = ChargeSound;
        m_AudioSource.playOnAwake = false;
        m_AudioSource.outputAudioMixerGroup =
            AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeBuildup);

        // create a second audio source, to play the sound with a delay
        m_AudioSourceLoop = gameObject.AddComponent<AudioSource>();
        m_AudioSourceLoop.clip = LoopChargeWeaponSfx;
        m_AudioSourceLoop.playOnAwake = false;
        m_AudioSourceLoop.loop = true;
        m_AudioSourceLoop.outputAudioMixerGroup =
            AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponChargeLoop);
    }

    void Update()
    {
        m_ChargeRatio = m_WeaponController.CurrentCharge;

        // update sound's volume and pitch
        if (m_ChargeRatio > 0)
        {
            if (!m_AudioSourceLoop.isPlaying &&
                 m_WeaponController.LastChargeTriggerTimestamp > m_LastChargeTriggerTimestamp)
            {
                m_LastChargeTriggerTimestamp = m_WeaponController.LastChargeTriggerTimestamp;
                if (!UseProceduralPitchOnLoopSfx)
                {
                    m_EndchargeTime = Time.time + ChargeSound.length;
                    m_AudioSource.Play();
                }

                m_AudioSourceLoop.Play();
            }

            if (!UseProceduralPitchOnLoopSfx)
            {
                float volumeRatio = Mathf.Clamp01((m_EndchargeTime - Time.time - FadeLoopDuration) / FadeLoopDuration);
                m_AudioSource.volume = volumeRatio;
                m_AudioSourceLoop.volume = 1 - volumeRatio;
            }
            else
            {
                m_AudioSourceLoop.pitch = Mathf.Lerp(1.0f, MaxProceduralPitchValue, m_ChargeRatio);
            }
        }
        else
        {
            m_AudioSource.Stop();
            m_AudioSourceLoop.Stop();
        }
    }
}
