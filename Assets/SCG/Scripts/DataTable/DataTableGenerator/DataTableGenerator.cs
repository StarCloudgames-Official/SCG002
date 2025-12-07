using UnityEngine;

[CreateAssetMenu(fileName = "DataTableGenerator", menuName = "SCG/DataTableGenerator")]
public class DataTableGenerator : ScriptableObject
{
    [Header("Output Settings")]
    public string generatedCodePath = "Assets/_Project/Generated/Data/";
}

