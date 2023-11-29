using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EjectionObjectScript : MonoBehaviour
{
    [SerializeField] private float ejectionForce = 10f;
    [SerializeField] [Range(0, 360)] private float ejectionAngleDegrees = 45f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 ejectionDirection = CalculateEjectionDirection();
            rb.velocity = ejectionDirection * ejectionForce;
        }
    }

    private Vector2 CalculateEjectionDirection()
    {
        float angleInRadians = ejectionAngleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    private void OnDrawGizmos()
    {
        Vector2 direction = CalculateEjectionDirection();
        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(direction.x, direction.y, 0) * 2; // Length of 2 units
        Gizmos.DrawLine(start, end);
    }
}
