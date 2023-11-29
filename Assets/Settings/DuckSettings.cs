using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DK/DuckySettings", fileName = "DuckySettings")]
public class DuckSettings : ScriptableObject
{
[Header("Ground Movement Settings")]
[Range(0,20), Tooltip ("Changes Walk Speed")] public float maxWalkSpeed = 7f;
[Range (0, 20), Tooltip ("Acceleration Rate for Walking")] public float walkAcceleration = 5f;
[Range(0,20), Tooltip ("Changes Max Run Speed")] public float maxRunSpeed = 10f;
[Range (0, 20), Tooltip ("Acceleration Rate for Running")] public float runAcceleration = 7f;
[Range (0, 50), Tooltip ("Deceleration Rate")] public float deceleration = 5f;
[Range (0, 2), Tooltip ("Velocity at which game then clamps and reduces velocity to 0")] public float velocityStopThreshold = .3f;
[Header("Air/Jump Settings")]
[Range(10,25), Tooltip ("Changes Jump Height")] public float jumpForce = 17f;
[Range(0,25), Tooltip ("Changes Max Speed in Air")] public float airMaxSpeed = 7f;
[Range(0,50), Tooltip ("Changes Jump Movement in Air")] public float airControlStrength = 7f;
[Range(0,100), Tooltip ("Changes Deceleration in Air")] public float airDeceleration = 50f;
[Range (0,1), Tooltip ("Time in Seconds for Jump Buffer")] public float jumpBufferTime = 0.2f;
[Range(0, 1), Tooltip ("Coyote Time In Seconds")] public float coyoteTime = 0.15f;
[Range (0,10), Tooltip ("Gravity Adjust on Jump Fall")] public float fallMultiplier = 1.4f;
[Range (0,50), Tooltip ("Strength of the Flap Mechanic")] public float flapStrength = 30f;
[Range (0,10), Tooltip ("Maximum Flap Time In Seconds")] public float maxFlapDuration = 1.5f;
[Header("Wall Jump Settings")]
[Range(0,50), Tooltip ("Changes Wall Jump Strength")] public float wallJumpForce = 10f;
[Range(0,180), Tooltip ("Changes Wall Jump Angle")] public float wallJumpAngle = 45f;
[Range(0,25), Tooltip ("Changes Wall Slide Speed")] public float wallSlideSpeed = 2f;
[Header("Other Settings")]
[Range(45, 90), Tooltip ("Highest Climable Angle")] public float steepestClimbableAngle = 60f;
[Range (0,10), Tooltip ("Time Faceplant Locks Input")] public float faceplantInputLockTime = 1f;
[Range (10, 50), Tooltip ("Velocity that Fall Speed is Clamped at")] public float maxFallVelocity = 20f;
[Range (0, 20), Tooltip ("Velocity at Which Player Considered Falling/Standing")] public float yVelocityBuffer = 10f;
[Range (0, 10), Tooltip ("The Falling Time at which the Player Will Then Faceplant")] public float deadThreshold = 2.5f;
}