using System.Collections;
using UnityEngine;

public class TeeterTotter : MonoBehaviour
{
    public float tiltSpeed = 10f;
    public float levelingSpeed = 5f;
    public float levelingDelay = 1.0f;  // 1 second delay
    public float angleTolerance = 1.0f; // Tolerance in degrees
    float targetAngle = 0f;
    private bool isPlayerOn = false;
    private bool shouldLevel = false;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = true;
            shouldLevel = false;  // Cancel leveling if the player is back on
            StopCoroutine("LevelingDelay");  // Stop the leveling delay coroutine

            float xPosDifference = collision.transform.position.x - transform.position.x;
            float tiltAmount = xPosDifference * tiltSpeed;
            rb.rotation += tiltAmount * Time.deltaTime;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
            StartCoroutine("LevelingDelay");  // Start the leveling delay coroutine
        }
    }

    IEnumerator LevelingDelay()
    {
        yield return new WaitForSeconds(levelingDelay);  // Wait for the delay duration
        shouldLevel = true;
    }

    void FixedUpdate()
    {
        if (!isPlayerOn && shouldLevel)
        {
            // Interpolate back to zero rotation
            float currentAngle = transform.eulerAngles.z;
        
            // Normalize the angle to a value between -180 and 180.
            if (currentAngle > 180)
            {
                currentAngle -= 360;
            }
            // Determine the closest angle among 0, 180 and -180.
            float[] possibleAngles = {0, 180, -180};
            bool isCloseEnough = false;
            foreach (float angle in possibleAngles)
            {
                if (Mathf.Abs(currentAngle - angle) <= angleTolerance)
                {
                    isCloseEnough = true;
                    transform.eulerAngles = new Vector3(0, 0, angle);  // Set to exact angle
                    break;
                }
            }

            if (!isCloseEnough)
            {
                float minDistance = Mathf.Abs(currentAngle - possibleAngles[0]);
                targetAngle = possibleAngles[0];
    
                for (int i = 1; i < possibleAngles.Length; i++)
                {
                    float distance = Mathf.Abs(currentAngle - possibleAngles[i]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetAngle = possibleAngles[i];
                    }
                }
    
                // Interpolate towards the closest angle.
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * levelingSpeed);
                transform.eulerAngles = new Vector3(0, 0, newAngle);
            }

    }
    }
}
