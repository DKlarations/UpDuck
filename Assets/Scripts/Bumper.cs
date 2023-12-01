using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumper : MonoBehaviour
{

    [SerializeField] private float bumpStrength = 10f;  // The strength of the bump
    [SerializeField] private float cooldownTime = 0.2f;  // Adjust the time as needed
    public AudioSource audioSource;
    [Header ("ANIMATION SETTINGS")]
    [SerializeField, Range(0f, 1f)] private float scaleDuration = 0.5f;
    [SerializeField, Range(1f, 4f)] private float maxScaleFactor = 2f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))  // Ensure the collided object is the player
        {
            Ducky playerController = collision.gameObject.GetComponent<Ducky>();

            if (playerController != null)
            {
                
                // Get the normal of the contact point
                Vector2 contactNormal = collision.GetContact(0).normal;

                // Apply the bump force
                //playerController.BumperBounce(bumpStrength,contactNormal);
                playerController.ApplyPushForce(-bumpStrength * contactNormal, cooldownTime);
                //playerController.SetHorizontalPush(contactNormal.x < 0 ? bumpStrength : -bumpStrength);

                StartCoroutine(ScaleBumper());
                PlayBumpSound();
            }
        }
    }
    private IEnumerator ScaleBumper()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * maxScaleFactor;

        // Scale up
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, EaseInOutQuint(t));
            yield return null;
        }

        // Scale down
        for (float t = 0; t < 1; t += Time.deltaTime / scaleDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, EaseInOutQuint(t));
            yield return null;
        }

        // Ensure it ends at the original scale
        transform.localScale = originalScale;
    }

    private float EaseInOutQuint(float x)
    {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    private void PlayBumpSound()
    {
        audioSource.Play();
    }

}
