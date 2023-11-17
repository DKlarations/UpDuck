using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DuckyMoverTool : EditorWindow
{
    private static bool toolActive = false;
    private static GameObject ducky;

    [MenuItem("Tools/Ducky Mover Tool %d")]
    public static void ToggleTool()
    {
        toolActive = !toolActive;

        if (toolActive)
        {
            SceneView.duringSceneGui += OnSceneGUI;
            ducky = Object.FindFirstObjectByType<Ducky>()?.gameObject;
            if (ducky == null)
            {
                Debug.LogError("Ducky not found in the scene!");
                toolActive = false;
            }
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (toolActive && ducky != null)
        {
            // Display an indicator in the Scene View
            Handles.BeginGUI();
            GUI.color = Color.green;
            GUILayout.Label("              Ducky Mover Tool Active (Double-click to exit)", new GUIStyle()
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.green }
            });
            Handles.EndGUI();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Detect double-click to deactivate the tool
            if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
            {
                toolActive = false;
                SceneView.duringSceneGui -= OnSceneGUI;
                Event.current.Use();
                return;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePosition = Event.current.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * ppp;
                mousePosition.x *= ppp;

                Vector2 worldPosition = sceneView.camera.ScreenToWorldPoint(mousePosition);

                if (Physics2D.Raycast(worldPosition, Vector2.zero))
                {
                    Undo.RecordObject(ducky.transform, "Move Ducky");
                    ducky.transform.position = worldPosition;
                    Event.current.Use();
                }
            }
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
}

#endif
