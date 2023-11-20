using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AmbientAudioSwitcher : MonoBehaviour
{
    public AudioClip audioClip;
    public AmbientAudioController audioController;

    void Start()
    {
    }
    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player")) 
        {
            audioController.ChangeTrack(audioClip);
        }
    }
    
}
