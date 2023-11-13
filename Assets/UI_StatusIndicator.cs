using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

public class UI_StatusIndicator : MonoBehaviour
{
    public TMP_Text label;
    public bool showInBuild;

    public void Start()
    {
        gameObject.SetActive(showInBuild);
    }
    public void LabelTheDuck(string newText)
    {
        label.text = newText;
    }
}