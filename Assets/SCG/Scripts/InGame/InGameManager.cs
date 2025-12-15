using StarCloudgamesLibrary;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InGameManager : Singleton<InGameManager>
{
    public InGameContext InGameContext { get; private set; }
    public UIInGameMain UIInGameMain { get; private set; }

    public override async Awaitable Initialize()
    {
        InGameContext = new InGameContext();
        InGameContext.Initialize(InGameSession.CurrentInGameEnterInfo);
        
        var characterGridManager = await AddressableExtensions.InstantiateAndGetComponent<CharacterGridManager>("CharacterGridManager");
        await characterGridManager.CreateGrid(5, 6);
        
        var spawnManager = SpawnManager.Create();
        await spawnManager.Initialize();
        
        InGameContext.CharacterGridManager = characterGridManager;

        UIInGameMain = await UIManager.OpenUI<UIInGameMain>();

        //set state by enterinfo
    }
}