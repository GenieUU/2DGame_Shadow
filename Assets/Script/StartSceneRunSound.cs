using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneRunSound : MonoBehaviour
{
    public AudioClip runClip;
    private AudioSource audioSource;

    void Start()
    {
        Time.timeScale = 1f;

        audioSource = GetComponent<AudioSource>();
        if (runClip != null)
        {
            audioSource.clip = runClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}