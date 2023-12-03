using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltBottom : MonoBehaviour
{
    private ConveyorBelt parentScript;

    private void Start()
    {
        // Get the ConveyorBelt script from the parent GameObject
        parentScript = GetComponentInParent<ConveyorBelt>();
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        parentScript.HandleTopAndBottomOfConveyorEnter(collider);
    }
    private void OnTriggerStay2D(Collider2D collider)
    {
        parentScript.HandleBottomOfConveyorStay(collider);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        parentScript.HandleTopAndBottomOfConveyorExit(collider);
    }
}
