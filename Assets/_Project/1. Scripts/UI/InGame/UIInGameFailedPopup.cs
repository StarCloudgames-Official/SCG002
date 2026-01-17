using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UIInGameFailedPopup : UIOverPopup
{
    [SerializeField] private UIRewardItemContainer rewardItemContainer;
    [SerializeField] private GameObject reviveButton;
    [SerializeField] private GameObject lobbyButton;

    private List<RewardData> rewardDatas;
    private InGameContext inGameContext;

    public override UniTask PreOpen(object param)
    {
        inGameContext = InGameManager.Instance.InGameContext;
        
        reviveButton.transform.localScale = Vector3.zero;
        lobbyButton.transform.localScale = Vector3.zero;
        
        reviveButton.SetActive(!inGameContext.Revived);
        
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

        if (reviveButton.activeSelf)
        {
            reviveButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        await lobbyButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).ToUniTask();
    }

    public void OnClickReviveButton()
    {
        inGameContext.StageManager.Revive();
        Close().Forget();
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