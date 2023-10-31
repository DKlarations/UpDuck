using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipperyPlatform : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
 //           collider.GetComponent<Ducky>().isOnSlipperyPlatform = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
 //           collider.GetComponent<Ducky>().isOnSlipperyPlatform = false;
        }
    }
}
