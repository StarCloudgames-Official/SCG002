using StarCloudgamesLibrary;
using UnityEngine;

public class InGameManager : Singleton<InGameManager>
{
    public InGameContext InGameContext { get; private set; }
    public UIInGameMain UIInGameMain { get; private set; }

    public override async Awaitable Initialize()
    {
        InGameContext = new InGameContext();
        InGameContext.Initialize(InGameSession.CurrentInGameEnterInfo);

        UIInGameMain = await UIManager.OpenUI<UIInGameMain>();

        //set state by enterinfo
    }
}