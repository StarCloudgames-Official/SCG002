#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering; // SerializedDictionary<,>

[CustomPropertyDrawer(typeof(SerializedDictionary<,>), true)]
public class SerializedDictionaryDrawer : PropertyDrawer
{
    private const float FoldoutHeight = 18f;

    private readonly System.Collections.Generic.Dictionary<string, ReorderableList> _lists
        = new System.Collections.Generic.Dictionary<string, ReorderableList>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var keys = property.FindPropertyRelative("m_Keys");

        if (!property.isExpanded || keys == null)
            return FoldoutHeight;

        var list = GetList(property);
        return FoldoutHeight + 2f + list.GetHeight();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var keys   = property.FindPropertyRelative("m_Keys");
        var values = property.FindPropertyRelative("m_Values");

        var foldoutRect = new Rect(position.x, position.y, position.width, FoldoutHeight);
        int count = (keys != null ? keys.arraySize : 0);
        property.isExpanded = EditorGUI.Foldout(
            foldoutRect,
            property.isExpanded,
            $"{label.text} ({count})",
            true
        );

        if (!property.isExpanded)
            return;

        if (keys == null || values == null)
        {
            EditorGUI.LabelField(
                new Rect(position.x, position.y + FoldoutHeight + 2f, position.width, FoldoutHeight),
                "SerializedDictionary backing fields not found (m_Keys / m_Values)"
            );
            return;
        }

        var list = GetList(property);

        var listRect = new Rect(
            position.x,
            position.y + FoldoutHeight + 2f,
            position.width,
            list.GetHeight()
        );

        list.DoList(listRect);
    }

    private ReorderableList GetList(SerializedProperty property)
    {
        var path = property.propertyPath;
        if (_lists.TryGetValue(path, out var existing))
            return existing;

        var keys   = property.FindPropertyRelative("m_Keys");
        var values = property.FindPropertyRelative("m_Values");

        var list = new ReorderableList(
            property.serializedObject,
            keys,
            draggable: true,
            displayHeader: false,
            displayAddButton: true,
            displayRemoveButton: true
        );

        list.elementHeight = EditorGUIUtility.singleLineHeight + 2f;
        list.headerHeight = 0f;

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (index < 0 || index >= keys.arraySize || index >= values.arraySize)
                return;

            var keyProp   = keys.GetArrayElementAtIndex(index);
            var valueProp = values.GetArrayElementAtIndex(index);

            rect.y += 1f;
            rect.height = EditorGUIUtility.singleLineHeight;

            float keyWidth   = rect.width * 0.35f;
            float spacing    = 4f;
            float valueWidth = rect.width - keyWidth - spacing;

            var keyRect   = new Rect(rect.x, rect.y, keyWidth, rect.height);
            var valueRect = new Rect(keyRect.xMax + spacing, rect.y, valueWidth, rect.height);

            EditorGUI.PropertyField(keyRect,   keyProp,   GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
        };

        // + 버튼
        list.onAddCallback = rl =>
        {
            Undo.RecordObject(property.serializedObject.targetObject, "Add Dictionary Entry");

            int newIndex = keys.arraySize;

            keys.arraySize++;
            values.arraySize++;

            var newKey   = keys.GetArrayElementAtIndex(newIndex);
            var newValue = values.GetArrayElementAtIndex(newIndex);

            // 새 Value는 기본값으로 초기화
            ResetProperty(newValue);

            // 새 Key를 "기존에 없는 값"으로 설정
            if (!TrySetUniqueKey(keys, newKey))
            {
                // 고유 키를 만들 수 없으면 롤백
                keys.arraySize--;
                values.arraySize--;

                EditorUtility.DisplayDialog(
                    "SerializedDictionary",
                    "더 이상 추가할 수 있는 고유 Key가 없습니다.",
                    "OK"
                );
            }

            property.serializedObject.ApplyModifiedProperties();
        };

        // - 버튼
        list.onRemoveCallback = rl =>
        {
            int index = rl.index;
            if (index < 0 || index >= keys.arraySize)
                return;

            Undo.RecordObject(property.serializedObject.targetObject, "Remove Dictionary Entry");

            keys.DeleteArrayElementAtIndex(index);
            values.DeleteArrayElementAtIndex(index);

            property.serializedObject.ApplyModifiedProperties();
        };

        // 드래그로 순서 바꿀 때 값 배열도 같이 이동
        list.onReorderCallbackWithDetails = (rl, oldIndex, newIndex) =>
        {
            if (oldIndex == newIndex) return;

            values.MoveArrayElement(oldIndex, newIndex);
            property.serializedObject.ApplyModifiedProperties();
        };

        _lists[path] = list;
        return list;
    }

    /// <summary>
    /// 새로 추가된 keyProp을 "기존에 없는 값"으로 설정한다.
    /// Enum / String만 고유값 보장. 나머지는 기본값만 넣는다.
    /// </summary>
    private static bool TrySetUniqueKey(SerializedProperty keys, SerializedProperty keyProp)
    {
        switch (keyProp.propertyType)
        {
            case SerializedPropertyType.Enum:
                {
                    var names = keyProp.enumDisplayNames;
                    int len = names.Length;

                    // 이미 사용 중인 enum 인덱스 체크
                    var used = new bool[len];
                    for (int i = 0; i < keys.arraySize; i++)
                    {
                        var p = keys.GetArrayElementAtIndex(i);
                        if (p.propertyType != SerializedPropertyType.Enum) continue;
                        int idx = p.enumValueIndex;
                        if (idx >= 0 && idx < len)
                            used[idx] = true;
                    }

                    // 아직 안 쓰인 enum 값 찾기
                    for (int i = 0; i < len; i++)
                    {
                        if (!used[i])
                        {
                            keyProp.enumValueIndex = i;
                            return true;
                        }
                    }

                    // 모든 enum 값을 이미 사용 중
                    return false;
                }

            case SerializedPropertyType.String:
                {
                    string baseName = "New Key";
                    string candidate = baseName;
                    int counter = 1;

                    while (ContainsStringKey(keys, candidate))
                    {
                        candidate = $"{baseName} {counter++}";
                    }

                    keyProp.stringValue = candidate;
                    return true;
                }

            default:
                // 기타 타입이면 그냥 기본값만 넣어준다 (중복은 사용자가 책임)
                ResetProperty(keyProp);
                return true;
        }
    }

    private static bool ContainsStringKey(SerializedProperty keys, string value)
    {
        for (int i = 0; i < keys.arraySize; i++)
        {
            var p = keys.GetArrayElementAtIndex(i);
            if (p.propertyType != SerializedPropertyType.String) continue;
            if (p.stringValue == value) return true;
        }

        return false;
    }

    /// <summary>
    /// SerializedProperty를 기본값으로 초기화.
    /// </summary>
    private static void ResetProperty(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                prop.intValue = 0;
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = 0f;
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = false;
                break;
            case SerializedPropertyType.String:
                prop.stringValue = string.Empty;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = 0;
                break;
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = null;
                break;
            case SerializedPropertyType.Color:
                prop.colorValue = Color.white;
                break;
            case SerializedPropertyType.Vector2:
                prop.vector2Value = Vector2.zero;
                break;
            case SerializedPropertyType.Vector3:
                prop.vector3Value = Vector3.zero;
                break;
            case SerializedPropertyType.Vector4:
                prop.vector4Value = Vector4.zero;
                break;
            case SerializedPropertyType.Rect:
                prop.rectValue = Rect.zero;
                break;
            case SerializedPropertyType.AnimationCurve:
                prop.animationCurveValue = AnimationCurve.Linear(0, 0, 1, 1);
                break;
            case SerializedPropertyType.Bounds:
                prop.boundsValue = new Bounds(Vector3.zero, Vector3.zero);
                break;
            default:
                // 다른 타입은 일단 그대로 둠
                break;
        }
    }
}
#endif
