using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltLeft : MonoBehaviour
{
    private ConveyorBelt parentScript;

    private void Start()
    {
        // Get the ConveyorBelt script from the parent GameObject
        parentScript = GetComponentInParent<ConveyorBelt>();
    }
    private void OnTriggerStay2D(Collider2D collider)
    {
        parentScript.HandleLeftOfConveyorStay(collider);
    }
}
