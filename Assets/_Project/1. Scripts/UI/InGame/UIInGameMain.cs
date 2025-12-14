using UnityEngine;

public class UIInGameMain : UIPanel
{
    private bool canSpawn = true;
    
    public void OnClickSpawn()
    {
        if (!canSpawn)
            return;

        SpawnManager.Instance.TrySpawnCharacter(() => canSpawn = true).Forget();
    }

    public void OnClickEnhance()
    {
        
    }

    public void OnClickSell()
    {
        
    }
}