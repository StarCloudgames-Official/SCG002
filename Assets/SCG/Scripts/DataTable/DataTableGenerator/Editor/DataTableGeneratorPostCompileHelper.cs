using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DataTableGeneratorPostCompileHelper
{
    private const string PendingKey = "SCG_DataTableGen_PendingBuild";

    static DataTableGeneratorPostCompileHelper()
    {
        if (SessionState.GetBool(PendingKey, false))
        {
            SessionState.EraseBool(PendingKey);
            EditorApplication.delayCall += RunSecondPhase;
        }
    }

    public static void ScheduleSecondPhase()
    {
        SessionState.SetBool(PendingKey, true);
    }

    private static void RunSecondPhase()
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:DataTableGenerator");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("[DataTable] DataTableGenerator asset not found. Cannot continue second phase.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<DataTableGenerator>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[DataTable] Could not load DataTableGenerator at {path}.");
                return;
            }

            var editor = Editor.CreateEditor(asset) as DataTableGeneratorEditor;
            if (editor == null)
            {
                Debug.LogWarning("[DataTable] Failed to create DataTableGeneratorEditor instance.");
                return;
            }

            EditorUtility.DisplayProgressBar("Google Sheet Build", "Continuing DataTable serialization...", 0.75f);
            editor.BuildAll(skipCodeGeneration: true).Forget();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataTable] Second-phase execution failed:\n{e}");
            EditorUtility.ClearProgressBar();
        }
    }
}

