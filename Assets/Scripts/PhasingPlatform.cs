using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhasingPlatform : MonoBehaviour
{
    public float visibleDuration = 2.25f;
    public float invisibleDuration = 2.25f;
    public float startOffset = 0.0f; // Offset before the platform starts its cycle
    private float timer;
    private SpriteRenderer spriteRenderer;
    private Collider2D collide;
    private bool isPlatformVisible = true;
    private bool isInitialized = false;
    public Animator animator;

    //================//
    //Animation States//
    //================//
    const string ON_ANIMATION = "PhasingPlatformOn";
    const string OFF_ANIMATION = "PhasingPlatformOff";

    void Start()
    {
        collide = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        timer = startOffset; // Start with the offset
    }

    void Update()
    {
        if (!isInitialized && timer <= 0)
        {
            // Initialize the platform state after offset
            animator.Play(ON_ANIMATION);
            collide.enabled = true;
            timer = visibleDuration;
            isPlatformVisible = true;
            isInitialized = true;
        }
        else if (isInitialized)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                if (isPlatformVisible)
                {
                    // Make platform invisible and disable collider
                    animator.Play(OFF_ANIMATION);
                    collide.enabled = false;
                    timer = invisibleDuration;
                    isPlatformVisible = false;
                }
                else
                {
                    // Make platform visible and enable collider
                    animator.Play(ON_ANIMATION);
                    collide.enabled = true;
                    timer = visibleDuration;
                    isPlatformVisible = true;
                }
            }
        }
        else
        {
            // Count down the offset timer
            timer -= Time.deltaTime;
        }
    }
}
