using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioSwitcher : MonoBehaviour
{
    public AudioClip audioClip;
    public AmbientAudioController audioController;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            audioController.ChangeTrack(audioClip);
        }
    }
}
