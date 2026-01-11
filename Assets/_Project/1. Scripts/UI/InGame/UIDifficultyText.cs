using System;
using TMPro;
using UnityEngine;

public class UIDifficultyText : MonoBehaviour
{
    [SerializeField] private TextColorSwapper textColorSwapper;
    [SerializeField] private TMP_Text difficultyText;

    public void SetText(DataTableEnum.Difficulty difficulty)
    {
        var targetswapType = (ISwapper.SwapType)(int)difficulty;
        textColorSwapper.Swap(targetswapType);
        difficultyText.text = LocalizationManager.Get(difficulty.ToString()).ToUpper();
    }
}