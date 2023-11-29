using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumFunnelActivation : MonoBehaviour
{
    private VacuumFunnel parentScript;

    private void Start()
    {
        // Get the VacuumFunnel script from the parent GameObject
        parentScript = GetComponentInParent<VacuumFunnel>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        parentScript.HandleEnterTrigger(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        parentScript.HandleExitTrigger(collider);
    }
    private void OnTriggerStay2D(Collider2D collider)
    {
        parentScript.HandleStayTrigger(collider);
    }
}
