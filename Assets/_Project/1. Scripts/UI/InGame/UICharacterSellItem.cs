using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSellItem : MonoBehaviour
{
    [SerializeField] private TMP_Text currentCountText;
    [SerializeField] private TMP_Text sellPriceText;
    [SerializeField] private TMP_Text spawnTypeText;

    [SerializeField] private Image portraitImage;

    private InGameContext inGameContext;
    private ClassTable currentClass;

    private int sellPrice;

    private int CurrentCount => inGameContext.SpawnManager.GetSpawnedCount(currentClass.classType, currentClass.spawnType);

    public void Set(DataTableEnum.ClassType classType, DataTableEnum.SpawnType spawnType)
    {
        inGameContext = InGameManager.Instance.InGameContext;

        currentClass = DataTableManager.Instance.GetClassTable(classType, spawnType);
        sellPrice = DataTableManager.Instance.GetSellPrice(spawnType);

        var spawnTypeLocalizedString = LocalizationManager.Get(spawnType.ToString());

        if (!string.IsNullOrEmpty(currentClass.portraitName))
        {
            portraitImage.SetSprite(AtlasType.CharacterPortrait, currentClass.portraitName);
        }

        currentCountText.text = ZString.Concat(CurrentCount);
        sellPriceText.text = ZString.Concat(sellPrice);
        spawnTypeText.text = spawnTypeLocalizedString;
    }

    public void OnClickSell()
    {
        if (!inGameContext.SpawnManager.SellCharacter(currentClass.classType, currentClass.spawnType))
            return;

        currentCountText.text = ZString.Concat(CurrentCount);
        inGameContext.InGameCrystal += sellPrice;
    }
}