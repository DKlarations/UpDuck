using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenPlatform : MonoBehaviour
{
    public Animator animator;
    //================//
    //Animation States//
    //================//
    const string HIDDEN_PLATFORM_ON_ANIMATION = "HiddenPlatformReveal";
    const string HIDDEN_PLATFORM_OFF_ANIMATION = "HiddenPlatformDisappear";
   
    public void TurnPlatformOn()
    {
        animator.Play(HIDDEN_PLATFORM_ON_ANIMATION);
    }
}
