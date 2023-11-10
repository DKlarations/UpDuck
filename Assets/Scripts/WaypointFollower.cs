using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour

{
    [SerializeField] private GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    private float previousXPosition;
    [SerializeField] private float speed = 2f;

    private void Start()
    {
        SetPreviousXPosition();
    }
    private void FixedUpdate()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
        SetPreviousXPosition();
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
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
