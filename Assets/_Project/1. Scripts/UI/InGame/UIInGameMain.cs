using System;
using TMPro;
using UnityEngine;

public class UIInGameMain : UIPanel
{
    [SerializeField] private ExtensionSlider killCountSlider;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text stageText;
    
    private bool canSpawn = true;
    private InGameContext inGameContext;

    public override async Awaitable PreOpen(object param)
    {
        inGameContext = param as InGameContext;
        
        //TODO : killCountSlider랑 timerText, stageText 다 StageManager에 이벤트로 연결해서 세팅하면 됨
        
        await Awaitable.NextFrameAsync();
    }
    
    public void OnClickSpawn()
    {
        if (!canSpawn)
            return;

        inGameContext.SpawnManager.TrySpawnCharacter(() => canSpawn = true).Forget();
    }

    public void OnClickEnhance()
    {
        
    }

    public void OnClickSell()
    {
        
    }
}