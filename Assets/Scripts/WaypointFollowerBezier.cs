using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollowerBezier : MonoBehaviour

{
    [SerializeField] private GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    private float previousXPosition;
    [SerializeField] private float speed = .5f;
    private float journeyLength;
    private float startTime;

    private void Start()
    {
        SetPreviousXPosition();
        UpdateWaypointDistance();
    }
    private void Update()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
            UpdateWaypointDistance();
        }
        SetPreviousXPosition();


        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        float easedStep = Mathf.SmoothStep(0.0f, 1.0f, fractionOfJourney); // This provides the ease-in and ease-out effect.

        transform.position = Vector2.Lerp(transform.position, waypoints[currentWaypointIndex].transform.position, easedStep);
    }
    private void UpdateWaypointDistance()
    {
        startTime = Time.time;
        journeyLength = Vector2.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);
    }

    public bool IsMovingRight()
    {
        return transform.position.x > previousXPosition;
    }
    private void SetPreviousXPosition()
    {
        previousXPosition = transform.position.x;
    }

}
