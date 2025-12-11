#region UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildConfigIOS", menuName = "SCG/BuildConfig/IOS")]
public class BuildConfigIOS : ScriptableObject
{
    [Header("Signing")]
    public string teamId;                    // Apple Developer Team ID
    public bool automaticSigning = true;     // 자동 서명 사용 여부
    public string provisioningProfileId;     // 수동 서명 시 프로비저닝 프로파일 UUID
    
    [Header("Player Settings")]
    public string bundleId;                  // Bundle Identifier
    public string versionName = "1.0.0";     // CFBundleShortVersionString
    public int buildNumber = 1;              // CFBundleVersion
    public bool autoBuildNumberUp = true;
    
    [Header("Build")]
    public string targetIOSVersion = "14.0"; // 최소 지원 iOS 버전
    
    [Header("Define Symbols")]
    public string[] scriptingDefineSymbols;

    [Header("Output")]
    public string outputDirectory = "../Builds/IOS";
}

#endregion