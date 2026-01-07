using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIBase : MonoBehaviour, IUI
{
    public virtual UniTask PreOpen(object param) => UniTask.CompletedTask;
    public virtual UniTask PreClose(object param) => UniTask.CompletedTask;

    public virtual async UniTask Open(object param = null)
    {
        await PreOpen(param);
        gameObject.SetActive(true);
    }

    public virtual async UniTask Close(object param = null)
    {
        await PreClose(param);
        UIManager.RemoveUI(this);
        Addressables.ReleaseInstance(gameObject);
    }

    public virtual void OnBackSpace()
    {
        Close().Forget();
    }
}
