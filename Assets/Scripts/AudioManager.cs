using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource audioSource;
    public AudioClip[] moveSounds;
    public AudioClip captureSound;
    public AudioClip winSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayMoveSound()
    {
        audioSource.clip = moveSounds[Random.Range(0, moveSounds.Length)];
        audioSource.Play();
    }

    public void PlayCaptureSound()
    {
        audioSource.clip = captureSound;
        audioSource.Play();
    }

    public void PlayWinSound()
    {
        audioSource.clip = winSound;
        audioSource.Play();
    }
}
