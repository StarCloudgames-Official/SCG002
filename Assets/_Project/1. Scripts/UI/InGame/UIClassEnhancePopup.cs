using UnityEngine;

public class UIClassEnhancePopup : MonoBehaviour
{
    [SerializeField] private UIClassEnhanceItem[] enhanceItems;

    public async Awaitable Set()
    {
        var inGameContext = InGameManager.Instance.InGameContext;

        var i = 0;
        foreach (var classType in inGameContext.ClassEnhancements.Keys)
        {
            if (i >= enhanceItems.Length)
                break;

            enhanceItems[i].Set(classType);
            i++;
        }

        await Awaitable.NextFrameAsync();
    }
}