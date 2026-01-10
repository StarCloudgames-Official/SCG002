#if UNITY_EDITOR

// Deprecated API 경고 무시 (BuildTargetGroup 관련)
#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    // ===========================
    // Android
    // ===========================

    // 메뉴에서도 쓸 수 있게 메뉴 아이템 추가
    [MenuItem("SCG/Build/Android (BuildConfigAOS 사용)")]
    public static void BuildAndroidMenu()
    {
        BuildAndroidWithConfig();
    }

    /// <summary>
    /// Jenkins에서 -executeMethod BuildScript.BuildAndroidWithConfig 로 호출하면 됨
    /// </summary>
    public static void BuildAndroidWithConfig()
    {
        var config = LoadBuildConfigAOS();
        if (config == null)
        {
            Debug.LogError("BuildScript: BuildConfigAOS 에셋을 찾지 못했습니다. t:BuildConfigAOS 검색 실패.");
            FailAndExit();
            return;
        }

        try
        {
            ApplyPlayerSettingsAndroid(config);

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("BuildScript: 활성화된 Build Settings 씬이 없습니다.");
                FailAndExit();
                return;
            }

            var outputDir = string.IsNullOrEmpty(config.outputDirectory)
                ? "Builds/AOS"
                : config.outputDirectory;

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            
            var ext = EditorUserBuildSettings.buildAppBundle ? "aab" : "apk";
            var productName = PlayerSettings.productName.Replace(' ', '_');
            var versionName = PlayerSettings.bundleVersion;
            var versionCode = PlayerSettings.Android.bundleVersionCode;

            var fileName = $"{productName}_{versionName}_{versionCode}.{ext}";
            var path = Path.Combine(outputDir, fileName);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"BuildScript: Android 빌드 성공 → {path}");
                SucceedAndExit();
            }
            else
            {
                Debug.LogError($"BuildScript: Android 빌드 실패. Result={summary.result}, Errors={summary.totalErrors}");
                FailAndExit();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"BuildScript: 예외 발생\n{e}");
            FailAndExit();
        }
    }

    private static BuildConfigAOS LoadBuildConfigAOS()
    {
        // 가장 첫 번째 BuildConfigAOS 에셋 사용
        var guids = AssetDatabase.FindAssets("t:BuildConfigAOS");
        if (guids == null || guids.Length == 0)
            return null;

        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<BuildConfigAOS>(path);
    }

    private static void ApplyPlayerSettingsAndroid(BuildConfigAOS config)
    {
        // Keystore
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = config.keystorePath;
        PlayerSettings.Android.keystorePass = config.keystorePassword;
        PlayerSettings.Android.keyaliasName = config.keyaliasName;
        PlayerSettings.Android.keyaliasPass = config.keyaliasPassword;

        // Application Id
        if (!string.IsNullOrEmpty(config.applicationId))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, config.applicationId);

        // Version
        if (!string.IsNullOrEmpty(config.versionName))
            PlayerSettings.bundleVersion = config.versionName;

        if (config.autoVersionCodeUp)
        {
            var newCode = PlayerSettings.Android.bundleVersionCode + 1;
            PlayerSettings.Android.bundleVersionCode = newCode;

            // Config에도 반영해서 다음 에디터 오픈 시 값 유지
            config.versionCode = newCode;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
        else
        {
            PlayerSettings.Android.bundleVersionCode = config.versionCode;
        }

        // AAB / APK
        EditorUserBuildSettings.buildAppBundle = config.useAppBundle;

        // Define Symbols
        var defines = ApplyDefineSymbols(
            BuildTargetGroup.Android,
            config.addDefineSymbols,
            config.removeDefineSymbols);

        Debug.Log($"BuildScript: PlayerSettings(AOS) 적용 완료. " +
                  $"Id={PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}, " +
                  $"Ver={PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode}), " +
                  $"AAB={EditorUserBuildSettings.buildAppBundle}, " +
                  $"Defines={defines}");
    }

    // ===========================
    // iOS
    // ===========================

    [MenuItem("SCG/Build/IOS (BuildConfigIOS 사용)")]
    public static void BuildIOSMenu()
    {
        BuildIOSWithConfig();
    }

    /// <summary>
    /// Jenkins에서 -executeMethod BuildScript.BuildIOSWithConfig 로 호출
    /// </summary>
    public static void BuildIOSWithConfig()
    {
        var config = LoadBuildConfigIOS();
        if (config == null)
        {
            Debug.LogError("BuildScript: BuildConfigIOS 에셋을 찾지 못했습니다. t:BuildConfigIOS 검색 실패.");
            FailAndExit();
            return;
        }

        try
        {
            ApplyPlayerSettingsIOS(config);

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("BuildScript: 활성화된 Build Settings 씬이 없습니다.");
                FailAndExit();
                return;
            }

            // 출력 디렉토리 (예: ../Build/IOS 설정 가능)
            var outputDir = string.IsNullOrEmpty(config.outputDirectory)
                ? "Builds/IOS"
                : config.outputDirectory;

            // 상대 경로 → 절대 경로 (Unity에서 CurrentDirectory = 프로젝트 루트)
            if (!Path.IsPathRooted(outputDir))
            {
                var projectPath = Directory.GetCurrentDirectory();
                outputDir = Path.GetFullPath(Path.Combine(projectPath, outputDir));
            }

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputDir,   // iOS는 "폴더" 경로
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"BuildScript: iOS 빌드 성공 → {outputDir}");
                SucceedAndExit();
            }
            else
            {
                Debug.LogError($"BuildScript: iOS 빌드 실패. Result={summary.result}, Errors={summary.totalErrors}");
                FailAndExit();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"BuildScript: iOS 빌드 중 예외 발생\n{e}");
            FailAndExit();
        }
    }

    private static BuildConfigIOS LoadBuildConfigIOS()
    {
        var guids = AssetDatabase.FindAssets("t:BuildConfigIOS");
        if (guids == null || guids.Length == 0)
            return null;

        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<BuildConfigIOS>(path);
    }

    private static void ApplyPlayerSettingsIOS(BuildConfigIOS config)
    {
        // Bundle Id
        if (!string.IsNullOrEmpty(config.bundleId))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, config.bundleId);
        }

        // Version (CFBundleShortVersionString)
        if (!string.IsNullOrEmpty(config.versionName))
        {
            PlayerSettings.bundleVersion = config.versionName;
        }

        // Build Number (CFBundleVersion)
        if (config.autoBuildNumberUp)
        {
            // 현재 PlayerSettings의 buildNumber 기반으로 증가
            int current;
            if (!int.TryParse(PlayerSettings.iOS.buildNumber, out current))
                current = config.buildNumber;

            var newNum = Math.Max(current, config.buildNumber) + 1;
            PlayerSettings.iOS.buildNumber = newNum.ToString();

            config.buildNumber = newNum;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
        else
        {
            PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();
        }

        // 최소 iOS 버전
        if (!string.IsNullOrEmpty(config.targetIOSVersion))
        {
            PlayerSettings.iOS.targetOSVersionString = config.targetIOSVersion;
        }

        // Signing
        if (!string.IsNullOrEmpty(config.teamId))
        {
            PlayerSettings.iOS.appleDeveloperTeamID = config.teamId; // Team ID
        }

        PlayerSettings.iOS.appleEnableAutomaticSigning = config.automaticSigning; // 자동/수동 전환

        if (!config.automaticSigning && !string.IsNullOrEmpty(config.provisioningProfileId))
        {
            // 수동 서명 모드용 프로비저닝 프로파일 UUID
            PlayerSettings.iOS.iOSManualProvisioningProfileID = config.provisioningProfileId;
            // 필요하면 iOSManualProvisioningProfileType도 추가로 세팅 가능
        }

        // Define Symbols
        var defines = ApplyDefineSymbols(
            BuildTargetGroup.iOS,
            config.addDefineSymbols,
            config.removeDefineSymbols);

        Debug.Log($"BuildScript: PlayerSettings(iOS) 적용 완료. " +
                  $"Id={PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS)}, " +
                  $"Ver={PlayerSettings.bundleVersion} ({PlayerSettings.iOS.buildNumber}), " +
                  $"Target iOS={PlayerSettings.iOS.targetOSVersionString}, " +
                  $"TeamID={PlayerSettings.iOS.appleDeveloperTeamID}, AutoSign={PlayerSettings.iOS.appleEnableAutomaticSigning}, " +
                  $"Defines={defines}");
    }

    // ===========================
    // 공통
    // ===========================

    private static void FailAndExit()
    {
        if (Application.isBatchMode)
            EditorApplication.Exit(1);
    }

    private static void SucceedAndExit()
    {
        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    private static string ApplyDefineSymbols(BuildTargetGroup targetGroup, string[] addSymbols, string[] removeSymbols)
    {
        // 기존 프로젝트 세팅에서 심볼 가져오기
        var existing = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        var symbols = new HashSet<string>(
            existing.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));

        // 추가
        if (addSymbols != null)
        {
            foreach (var s in addSymbols)
            {
                if (!string.IsNullOrEmpty(s))
                    symbols.Add(s);
            }
        }

        // 제거
        if (removeSymbols != null)
        {
            foreach (var s in removeSymbols)
            {
                if (!string.IsNullOrEmpty(s))
                    symbols.Remove(s);
            }
        }

        var result = string.Join(";", symbols);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, result);
        return result;
    }
}

#endif