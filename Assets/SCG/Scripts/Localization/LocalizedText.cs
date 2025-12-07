using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;
    
    private TextMeshProUGUI localizedText;

    private void Awake()
    {
        localizedText = GetComponent<TextMeshProUGUI>();
        localizedText.text = key.Localize();
    }
}