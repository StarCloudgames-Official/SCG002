#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class PlayFromFirstBuildSceneToolbar
{
    private static IMGUIContainer imguiContainer;
    private static string buttonText = "▶ (No Scene)";
    private static bool hasScene;

    private static GUIStyle mainButtonStyle;   // 첫 씬 Play 버튼
    private static GUIStyle dropdownStyle;     // 씬 이동 드롭다운 버튼

    private static string[] sceneNames = { "No Scenes" };
    private static string[] scenePaths = System.Array.Empty<string>();
    private static int selectedSceneIndex = 0;

    // 드롭다운 라벨 캐시
    private static string CurrentSceneLabel =>
        scenePaths.Length == 0
            ? "No Scenes"
            : sceneNames[Mathf.Clamp(selectedSceneIndex, 0, sceneNames.Length - 1)];

    static PlayFromFirstBuildSceneToolbar()
    {
        EditorApplication.update += TryCreateToolbarElement;
        EditorBuildSettings.sceneListChanged += UpdateButtonLabel;
        EditorApplication.projectChanged += RefreshSceneList;

        UpdateButtonLabel();
        RefreshSceneList();
    }

    private static void TryCreateToolbarElement()
    {
        if (imguiContainer != null) return;

        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;

        var toolbarInstance = toolbars[0];

        var rootField = toolbarType.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);
        var root = rootField?.GetValue(toolbarInstance) as VisualElement;
        if (root == null) return;

        var playModeZone = root.Q<VisualElement>("ToolbarZonePlayMode");
        if (playModeZone == null) return;

        imguiContainer = new IMGUIContainer(OnToolbarGUI)
        {
            name = "PlayFromFirstBuildScene_IMGUI"
        };

        // 전체 그룹과 기본 Play 버튼 사이 간격
        imguiContainer.style.marginLeft = 6;
        imguiContainer.style.marginRight = 16; // 이걸 키우면 기본 Play 버튼과 더 멀어짐
        imguiContainer.style.alignItems = Align.Center;

        playModeZone.Insert(0, imguiContainer);
    }

    private static void EnsureStyles()
    {
        if (mainButtonStyle != null && dropdownStyle != null) return;

        // 기본 툴바 버튼 스타일 복사
        var baseStyle = new GUIStyle(EditorStyles.toolbarButton);

        // 세로 위치를 툴바와 일치시키기 위해 위/아래 margin은 그대로 두고
        // 좌우만 약간 수정
        mainButtonStyle = new GUIStyle(baseStyle)
        {
            alignment   = TextAnchor.MiddleCenter,
            fixedHeight = baseStyle.fixedHeight
        };
        mainButtonStyle.margin = new RectOffset(
            baseStyle.margin.left,
            baseStyle.margin.right,
            baseStyle.margin.top,
            baseStyle.margin.bottom);
        mainButtonStyle.padding = new RectOffset(
            baseStyle.padding.left,
            baseStyle.padding.right,
            baseStyle.padding.top,
            baseStyle.padding.bottom);

        // 드롭다운도 같은 스타일을 쓰되 폭만 더 작게
        dropdownStyle = new GUIStyle(mainButtonStyle);
    }

    private static void OnToolbarGUI()
    {
        EnsureStyles();

        GUILayout.BeginHorizontal();

        // 첫 씬 Play 버튼
        using (new EditorGUI.DisabledScope(!hasScene))
        {
            if (GUILayout.Button(buttonText, mainButtonStyle, GUILayout.MinWidth(50)))
            {
                OnClickPlayFirstBuildScene();
            }
        }

        // 첫 씬 버튼과 드롭다운 사이 간격
        GUILayout.Space(4);

        // 드롭다운 버튼 (텍스트 + ▾)
        using (new EditorGUI.DisabledScope(scenePaths.Length == 0))
        {
            string label = $"{CurrentSceneLabel} ▾";

            // 드롭다운이 더 작게 보이도록 최대 폭 제한
            if (GUILayout.Button(label, dropdownStyle,
                    GUILayout.MinWidth(40), GUILayout.MaxWidth(50)))
            {
                ShowSceneMenu();
            }
        }

        GUILayout.EndHorizontal();
    }

    #region 데이터 갱신

    private static void UpdateButtonLabel()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            hasScene = false;
            buttonText = "▶ (No Scene)";
            return;
        }

        hasScene = true;
        string scenePath = scenes[0].path;
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        buttonText = $"▶ {sceneName}";
    }

    private static void RefreshSceneList()
    {
        // Assets 폴더 아래의 씬만 검색 (패키지 테스트용 씬 제거)
        string[] searchInFolders = { "Assets/_Project/0. Scene" };
        string[] guids = AssetDatabase.FindAssets("t:Scene", searchInFolders);

        if (guids == null || guids.Length == 0)
        {
            sceneNames = new[] { "No Scenes" };
            scenePaths = System.Array.Empty<string>();
            selectedSceneIndex = 0;
            return;
        }

        sceneNames = new string[guids.Length];
        scenePaths = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(path);

            scenePaths[i] = path;
            sceneNames[i] = name;
        }

        string activePath = EditorSceneManager.GetActiveScene().path;
        int index = System.Array.IndexOf(scenePaths, activePath);
        selectedSceneIndex = Mathf.Max(0, index);
    }

    #endregion

    #region 동작

    private static void OnClickPlayFirstBuildScene()
    {
        if (!hasScene) return;

        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0) return;

        string scenePath = scenes[0].path;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var opened = EditorSceneManager.OpenScene(scenePath);
        if (!opened.IsValid())
        {
            Debug.LogError($"[PlayFromFirstBuildSceneToolbar] 씬을 열 수 없습니다: {scenePath}");
            return;
        }

        EditorApplication.isPlaying = true;
    }

    private static void ShowSceneMenu()
    {
        var menu = new GenericMenu();

        if (scenePaths.Length == 0)
        {
            menu.AddDisabledItem(new GUIContent("No Scenes Found"));
        }
        else
        {
            for (int i = 0; i < sceneNames.Length; i++)
            {
                int index = i;
                bool on = (index == selectedSceneIndex);
                menu.AddItem(new GUIContent(sceneNames[i]), on, () => OnSelectScene(index));
            }
        }

        // 마지막 버튼 rect 기준으로 드롭다운 표시
        var rect = GUILayoutUtility.GetLastRect();
        menu.DropDown(rect);
    }

    private static void OnSelectScene(int index)
    {
        if (scenePaths.Length == 0) return;

        index = Mathf.Clamp(index, 0, scenePaths.Length - 1);
        string targetPath = scenePaths[index];

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var opened = EditorSceneManager.OpenScene(targetPath);
        if (!opened.IsValid())
        {
            Debug.LogError($"[PlayFromFirstBuildSceneToolbar] 씬을 열 수 없습니다: {targetPath}");
            return;
        }

        selectedSceneIndex = index;
    }

    #endregion
}
#endif
