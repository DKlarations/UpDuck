using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float delayBeforeDisable = 2.0f; // Delay before disabling (adjust as needed)
    [SerializeField] private float delayBeforeEnable = 2.0f; // Delay before re-enabling (adjust as needed)

    // Declare constants for your colors
    private Color activePlatformColor = new Color(51 / 255f, 193 / 255f, 250 / 255f, 1.0f); // 33C1FA
    private Color deactivatedPlatformColor = new Color(20 / 255f, 73 / 255f, 94 / 255f, 1.0f); // 14495E

    public AudioSource audioPlayer;
    public AudioClip iceCrackingSounds;
    public AudioClip iceReturningSounds;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Ducky")
        {
            StartCoroutine(DisableAndEnablePlatform());
        }
    }

    public IEnumerator DisableAndEnablePlatform()
    {
        // Delay before disabling the platform
        yield return new WaitForSeconds(delayBeforeDisable);

        // Disable the Box Collider
        boxCollider.enabled = false;

        // Change the sprite color using the deactivatedPlatformColor constant
        spriteRenderer.color = deactivatedPlatformColor;

        //Play Ice Cracking Sounds
        AudioClip clipToPlay = iceCrackingSounds;
        audioPlayer.clip = clipToPlay;
        audioPlayer.Play(); 

        // Delay before re-enabling the platform
        yield return new WaitForSeconds(delayBeforeEnable);

        // Re-enable the Box Collider
        boxCollider.enabled = true;

        // Reset the sprite color to the activePlatformColor constant
        spriteRenderer.color = activePlatformColor;

        //Play Ice Returning Sounds
        audioPlayer.clip = iceReturningSounds;
        audioPlayer.Play(); 
    }
}
