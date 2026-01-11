using UnityEditor;
using UnityEngine;

public static class UnityHotKeys
{
    [MenuItem("Edit/Toggle Selected Objects Active %t")]
    private static void ToggleSelectedObjectsActive()
    {
        var selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
            return;

        foreach (var obj in selectedObjects)
        {
            Undo.RecordObject(obj, "Toggle Active");
            obj.SetActive(!obj.activeSelf);
        }
    }

    [MenuItem("Edit/Toggle Selected Objects Active %t", true)]
    private static bool ToggleSelectedObjectsActiveValidate()
    {
        return Selection.gameObjects.Length > 0;
    }
}