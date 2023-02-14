using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip grassClip;
    public AudioClip metalClip;
    public AudioClip woodClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check the tag of the surface the player is currently on
        if (gameObject.tag == "Grass")
        {
            audioSource.clip = grassClip;
            audioSource.Play();
        }
        else if (gameObject.tag == "Metal")
        {
            audioSource.clip = metalClip;
            audioSource.Play();
        }
        else if (gameObject.tag == "Wood")
        {
            audioSource.clip = woodClip;
            audioSource.Play();
        }
    }
}