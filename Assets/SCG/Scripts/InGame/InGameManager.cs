using Cysharp.Threading.Tasks;
using StarCloudgamesLibrary;

public class InGameManager : Singleton<InGameManager>
{
    public InGameContext InGameContext { get; private set; }
    public UIInGameMain UIInGameMain { get; private set; }

    public override async UniTask Initialize()
    {
        InGameContext = new InGameContext();
        InGameContext.Initialize(InGameSession.CurrentInGameEnterInfo);

        var stageManager = new StageManager();
        var stageData = DataTableManager.Instance.GetStageDataTable(InGameContext.EnterInfo.Stage, InGameContext.EnterInfo.Floor);
        await stageManager.Initialize(stageData);

        var characterGridManagerPath = AddressableExtensions.CharacterGridManagerPath;
        var characterGridManager = await AddressableExtensions.InstantiateAndGetComponent<CharacterGridManager>(characterGridManagerPath);
        await characterGridManager.CreateGrid(5, 6);

        var spawnManager = new SpawnManager();
        await spawnManager.Initialize();

        InGameContext.CharacterGridManager = characterGridManager;
        InGameContext.StageManager = stageManager;
        InGameContext.SpawnManager = spawnManager;

        await SCGObjectPoolingManager.CreatePoolAsync<Projectile>("Projectile", CachedTransform, 50);

        UIInGameMain = await UIManager.OpenUI<UIInGameMain>(InGameContext);
    }
}
