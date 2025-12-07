using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIBase : MonoBehaviour, IUI
{
    public virtual Awaitable PreOpen() => null;
    public virtual Awaitable PreClose() => null;

    public virtual async Awaitable Open(object param = null)
    {
        var preOpen = PreOpen();
        if (preOpen != null)
            await preOpen;

        gameObject.SetActive(true);
    }

    public virtual async Awaitable Close(object param = null)
    {
        var preClose = PreClose();
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