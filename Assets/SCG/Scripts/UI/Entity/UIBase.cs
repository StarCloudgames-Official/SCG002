using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIBase : MonoBehaviour, IUI
{
    public virtual Awaitable PreOpen(object param) => null;
    public virtual Awaitable PreClose(object param) => null;

    public virtual async Awaitable Open(object param = null)
    {
        var preOpen = PreOpen(param);
        if (preOpen != null)
            await preOpen;

        gameObject.SetActive(true);
    }

    public virtual async Awaitable Close(object param = null)
    {
        var preClose = PreClose(param);
        if (preClose != null)
            await preClose;

        UIManager.RemoveUI(this);
        Addressables.ReleaseInstance(gameObject);
    }

    public virtual void OnBackSpace()
    {
        Close().Forget();
    }
}