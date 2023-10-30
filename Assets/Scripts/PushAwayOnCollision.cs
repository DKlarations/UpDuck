using UnityEngine;

public class PushAwayOnCollision : MonoBehaviour
{
    [SerializeField] private float pushForce = 30f;  // Adjust the force as needed

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 pushDirection = collision.transform.position - transform.position;
            pushDirection.y = 0;
            pushDirection.Normalize();

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                //playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
                playerRb.AddForce(new Vector2(50,0), ForceMode2D.Impulse);

                Debug.Log("Push force applied: " + pushDirection);  // This will confirm that the force is being applied
            }
        }
    }
}