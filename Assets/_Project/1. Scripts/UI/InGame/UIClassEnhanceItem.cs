using TMPro;
using UnityEngine;

public class UIClassEnhanceItem : MonoBehaviour
{
    [SerializeField] private TMP_Text currentCountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text classText;
    
    private DataTableEnum.ClassType classType;
    private InGameContext inGameContext;

    public void Set(DataTableEnum.ClassType classType)
    {
        this.classType = classType;
        
        inGameContext = InGameManager.Instance.InGameContext;
        
        var currentLevel = inGameContext.GetClassEnhanceLevel(classType);
        var currentCount = inGameContext.SpawnManager.GetSpawnedCount(classType);
        var price = DataTableManager.Instance.GetClassEnhancePrice(currentLevel);
        
        levelText.text = $"Lv. {currentLevel}";
        priceText.text = price == null ? "MAX" : price.Value.ToString();
        classText.text = LocalizationManager.Get(classType.ToString());
        currentCountText.text = currentCount.ToString();
    }

    public void OnClickEnhance()
    {
        var currentLevel = inGameContext.GetClassEnhanceLevel(classType);
        var nextLevel = currentLevel + 1;

        if (!DataTableManager.Instance.GetClassEnhanceRatio(nextLevel).HasValue)
            return;

        var enhancePrice = DataTableManager.Instance.GetClassEnhancePrice(currentLevel);
        if (enhancePrice == null)
            return;

        if (!inGameContext.CanUseInGameCrystal(enhancePrice.Value))
            return;

        inGameContext.UseInGameCrystal(enhancePrice.Value);
        inGameContext.EnhanceClass(classType);
        Set(classType);
    }
}