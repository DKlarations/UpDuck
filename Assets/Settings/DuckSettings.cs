using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DK/DuckySettings", fileName = "DuckySettings")]
public class DuckSettings : ScriptableObject
{
[Header("Movement")]
[Range(0,20), Tooltip ("Changes Walk Speed")] public float originalWalkSpeed = 7f;
[Range(10,25), Tooltip ("Changes Jump Height")] public float jumpSpeed = 17f;
[Range(45, 90), Tooltip ("Highest Climable Angle")] public float steepestClimbableAngle = 60f;
[Range(0, 1), Tooltip ("Coyote Time In Seconds")] public float coyoteTime = 0.15f;
[Range (0,50), Tooltip ("Strength of the Flap Mechanic")] public float flapStrength = 30f;
[Range (0,10), Tooltip ("Maximum Flap Time In Seconds")] public float maxFlapDuration = 1.5f;
[Range (0,10), Tooltip ("Time Faceplant Locks Input")] public float faceplantInputLockTime = 1f;
[Range (0,10), Tooltip ("Time in Seconds for Jump Buffer")] public float jumpBufferTime = 0.2f;
[Range (0,10), Tooltip ("Gravity Adjust on Jump Fall")] public float fallMultiplier = 1.4f;
[Range (10, 50), Tooltip ("Velocity that Fall Speed is Clamped at")] public float maxFallVelocity = 20f;
[Range (0, 20), Tooltip ("Velocity at Which Player Considered Falling/Standing")] public float yVelocityBuffer = 10f;
[Range (0, 10), Tooltip ("The Falling Time at which the Player Will Then Faceplant")] public float deadThreshold = 2.5f;
}