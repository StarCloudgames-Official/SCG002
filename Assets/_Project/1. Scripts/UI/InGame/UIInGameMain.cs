using UnityEngine;

public class UIInGameMain : UIPanel
{
    private bool canSpawn = true;
    private InGameContext inGameContext;

    public override async Awaitable PreOpen(object param)
    {
        inGameContext = param as InGameContext;
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