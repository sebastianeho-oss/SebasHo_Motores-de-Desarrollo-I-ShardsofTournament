using UnityEngine;

public class WeaponSoundController : MonoBehaviour
{
    [Header("Componente de Audio")]
    public AudioSource weaponAudioSource;

    [Header("Clips de Audio del Arma")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound; // Sonido cuando intentas disparar sin munición

    void Start()
    {
        if (weaponAudioSource == null)
        {
            weaponAudioSource = GetComponent<AudioSource>();
            if (weaponAudioSource == null)
            {
                weaponAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void PlayShootSound()
    {
        if (weaponAudioSource != null && shootSound != null)
        {
            weaponAudioSource.PlayOneShot(shootSound);
        }
    }

    public void PlayReloadSound()
    {
        if (weaponAudioSource != null && reloadSound != null)
        {
            weaponAudioSource.PlayOneShot(reloadSound);
        }
    }

    public void PlayEmptyClickSound()
    {
        if (weaponAudioSource != null && emptyClickSound != null)
        {
            weaponAudioSource.PlayOneShot(emptyClickSound);
        }
    }
}