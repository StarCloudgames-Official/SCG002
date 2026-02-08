using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class UIInGameClearPopup : UIOverPopup
{
    [SerializeField] private UIRewardItemContainer rewardItemContainer;
    [SerializeField] private GameObject lobbyButton;

    private List<RewardData> rewardDatas;
    private InGameContext inGameContext;

    public override UniTask PreOpen(object param)
    {
        inGameContext = InGameManager.Instance.InGameContext;
        
        lobbyButton.transform.localScale = Vector3.zero;
        
        return UniTask.CompletedTask;
    }

    public override async UniTask Open(object param)
    {
        await base.Open(param);
        
        rewardDatas = param as List<RewardData>;
        await rewardItemContainer.SetUpContainer(rewardDatas);
        await ButtonProduce();
    }

    private async UniTask ButtonProduce()
    {
        await LMotion.Create(Vector3.zero, Vector3.one, 0.5f).WithEase(Ease.OutBack).BindToLocalScale(lobbyButton.transform).ToUniTask();
    }

    public void OnClickToLobbyButton()
    {
        DatabaseManager.Instance.AddRewardList(rewardDatas);
        InGameSession.LeaveInGame();
    }
    
    public override void OnBackSpace()
    {
    }
}