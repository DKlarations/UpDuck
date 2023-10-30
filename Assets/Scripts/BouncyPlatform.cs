// BouncyPlatform.cs
using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f; // Set desired bounce force

    public float GetBounceForce()
    {
        return bounceForce;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Ducky playerController = collision.gameObject.GetComponent<Ducky>();
                if (playerController != null)
                {
                    playerController.Bounce(bounceForce);
                }
            }
        }
    }
}
