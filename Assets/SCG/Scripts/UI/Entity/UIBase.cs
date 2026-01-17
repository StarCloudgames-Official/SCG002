using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIBase : MonoBehaviour, IUI
{
    [SerializeField] private Animator animator;
    
    private const string OpenAnimationHash = "Open";
    private const string CloseAnimationHash = "Close";
    
    public virtual UniTask PreOpen(object param) => UniTask.CompletedTask;
    public virtual UniTask PreClose(object param) => UniTask.CompletedTask;

    public virtual async UniTask Open(object param = null)
    {
        await PreOpen(param);

        gameObject.SetActive(true);
        
        if(!animator)
            return;
        
        await animator.WaitAfterPlay(OpenAnimationHash);
    }

    public virtual async UniTask Close(object param = null)
    {
        await PreClose(param);

        if (animator)
        {
            await animator.WaitAfterPlay(CloseAnimationHash);
        }

        UIManager.RemoveUI(this);
        Addressables.ReleaseInstance(gameObject);
    }

    public virtual void OnBackSpace()
    {
        Close().Forget();
    }
}
