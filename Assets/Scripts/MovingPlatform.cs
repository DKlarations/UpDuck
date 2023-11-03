using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Animator animator;
    private float previousXPosition;
    WaypointFollower waypointFollower;
    //================
    //Animation States
    //================
    const string RIGHT_ANIMATION = "MovingPlatformRight";
    const string LEFT_ANIMATION = "MovingPlatformLeft";
    void Start()
    {
        waypointFollower = gameObject.GetComponent<WaypointFollower>();
        animator = GetComponent<Animator>();
    }
  
    void FixedUpdate()
    {
        if (waypointFollower.IsMovingRight())
        {
            animator.Play(RIGHT_ANIMATION);    
        }
        else 
        {
            animator.Play(LEFT_ANIMATION);
        }    
 
    }



}
