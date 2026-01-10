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
    [Tooltip("프로젝트 세팅에 추가할 심볼")]
    public string[] addDefineSymbols;
    [Tooltip("프로젝트 세팅에서 제거할 심볼")]
    public string[] removeDefineSymbols;

    [Header("Output")]
    public string outputDirectory = "Builds/AOS";
}

#endregion