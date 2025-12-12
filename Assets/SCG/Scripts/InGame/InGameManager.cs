using StarCloudgamesLibrary;
using UnityEngine;

public class InGameManager : Singleton<InGameManager>
{
    public InGameContext InGameContext { get; private set; }

    public override async Awaitable Initialize()
    {
        InGameContext = new InGameContext();
        
        InGameContext.EnterInfo = InGameSession.CurrentInGameEnterInfo;
        
        //set state by enterinfo
    }
}