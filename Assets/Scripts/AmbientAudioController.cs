using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioController : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioClip forestAmbient; // Array of your ambient tracks
    public float fadeTime = 2.0f; // Time it takes to fade in/out
    private bool isFading = false;

    void Start()
    {
        // Start the first track
        audioSource1.clip = forestAmbient;
        audioSource1.volume = 1.0f;
        audioSource2.volume = 0.0f;

        audioSource1.Play();
    }

    void Update()
    {

    }
    public void ChangeTrack(AudioClip newTrack)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAudio(newTrack));
        }
    }

    private IEnumerator FadeAudio(AudioClip newTrack)
    {
        isFading = true;

        // Determine which source is currently active and which is inactive
        AudioSource activeSource = audioSource1.isPlaying ? audioSource1 : audioSource2;
        AudioSource newSource = activeSource == audioSource1 ? audioSource2 : audioSource1;

        // Set up the new track
        newSource.clip = newTrack;
        newSource.Play();

        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(1f, 0f, timer / fadeTime);
            newSource.volume = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }

        // Stop the old track and finalize volumes
        activeSource.Stop();
        activeSource.volume = 0f;
        newSource.volume = 1f;

        isFading = false;
    }
}

