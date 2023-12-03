using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltActivation : MonoBehaviour
{
    private ConveyorBelt parentScript;

    private void Start()
    {
        // Get the ConveyorBelt script from the parent GameObject
        parentScript = GetComponentInParent<ConveyorBelt>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        parentScript.HandleProximityEnter(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        parentScript.HandleProximityExit(collider);
    }
}
