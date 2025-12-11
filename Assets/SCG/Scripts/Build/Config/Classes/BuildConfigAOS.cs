#region UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildConfigAOS", menuName = "SCG/BuildConfig/AOS")]
public class BuildConfigAOS : ScriptableObject
{
    [Header("Keystore")]
    public string keystorePath;
    public string keystorePassword;
    public string keyaliasName;
    public string keyaliasPassword;

    [Header("Player Settings")]
    public string applicationId;
    public string versionName = "1.0.0";
    public int versionCode = 1;
    public bool autoVersionCodeUp = true;

    [Header("Build")]
    public bool useAppBundle = true;
    
    [Header("Define Symbols")]
    public string[] scriptingDefineSymbols;

    [Header("Output")]
    public string outputDirectory = "Builds/AOS";
}

#endregion