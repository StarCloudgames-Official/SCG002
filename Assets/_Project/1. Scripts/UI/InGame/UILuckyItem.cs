using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILuckyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text spawnTypeText;
    [SerializeField] private TMP_Text chanceText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image portraitImage;

    private LuckyDataTable data;
    private InGameContext inGameContext;

    public void Set(LuckyDataTable data)
    {
        this.data = data;
        inGameContext = InGameManager.Instance.InGameContext;

        var spawnTypeString = LocalizationManager.Get(data.spawnType.ToString());
        var chanceString = $"{data.spawnChance * 100.0f:F0}%";
        var priceString = data.pricePoint.ToString();
        
        spawnTypeText.text = spawnTypeString;
        chanceText.text = chanceString;
        priceText.text = priceString;
        
        portraitImage.SetSprite(AtlasType.CharacterPortrait, data.portraitName);
    }

    public void OnClickLucky()
    {
        if(!inGameContext.SpawnManager.CanSpawnCharacter())
            return;
        if (!inGameContext.CanUseLuckyPoint(data.pricePoint))
            return;
        
        inGameContext.UseLuckyPoint(data.pricePoint);

        var randomChance = Random.value;
        if (randomChance > data.spawnChance)
        {
            //TODO : Failed toast
            return;
        }

        var randomClass = inGameContext.SpawnManager.GetRandomClassType();
        inGameContext.SpawnManager.SpawnCharacter(randomClass, data.spawnType);

        //TODO : success toast
    }
}