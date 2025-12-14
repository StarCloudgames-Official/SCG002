using UnityEngine;

public class UIInGameMain : UIPanel
{
    public void OnClickSpawn()
    {
        SpawnManager.TryGetInGameSpawnType(out var spawnType);
        Debug.Log(spawnType);
    }

    public void OnClickEnhance()
    {
        
    }

    public void OnClickSell()
    {
        
    }
}