using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPlatform : MonoBehaviour
{
    public GameTimer gameTimer;
    public void OnTriggerEnter2D(Collider2D collider)
    {
        gameTimer.StopTimer();
    }
}
