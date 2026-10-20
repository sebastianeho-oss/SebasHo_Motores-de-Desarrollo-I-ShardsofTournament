using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    public AudioSource audioSource;             // Para sonidos cortos (salto, daño, muerte)
    public AudioSource footstepsAudioSource;    // AudioSource exclusivo para los pasos
    public AudioClip jumpSound;
    public AudioClip moveSound;
    public AudioClip takingDamageSound;
    public AudioClip deathSound;

    public void playJump()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    public void playMove()
    {
        if (footstepsAudioSource != null && moveSound != null)
        {
            if (!footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.clip = moveSound;
                footstepsAudioSource.loop = true;
                footstepsAudioSource.Play();
            }
        }
    }

    public void stopMove()
    {
        if (footstepsAudioSource != null && footstepsAudioSource.isPlaying)
        {
            footstepsAudioSource.Stop();
        }
    }

    public void playDamage()
    {
        if (audioSource != null && takingDamageSound != null)
        {
            audioSource.PlayOneShot(takingDamageSound);
        }
    }

    public void playDeath()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
}