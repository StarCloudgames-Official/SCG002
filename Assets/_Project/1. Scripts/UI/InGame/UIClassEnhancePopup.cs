using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIClassEnhancePopup : MonoBehaviour
{
    [SerializeField] private UIClassEnhanceItem[] enhanceItems;

    public async UniTask Set()
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

        await UniTask.NextFrame();
    }
}
