using UnityEngine;

public class UILuckyPopup : MonoBehaviour
{
    [SerializeField] private UILuckyItem[] luckyItems;

    public async Awaitable Set()
    {
        var allData = DataTableManager.Instance.GetAllLuckyDataTables();
        for (var i = 0; i < allData.Count; i++)
        {
            if(i >= luckyItems.Length)
                break;
            if(luckyItems[i] == null)
                continue;

            luckyItems[i].Set(allData[i]);
        }

        await Awaitable.NextFrameAsync();
    }
}