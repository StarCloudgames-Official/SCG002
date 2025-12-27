using UnityEngine;

public class MonsterHealth : CachedMonoBehaviour
{
    [SerializeField] private ObjectSlider healthBar;

    private MonsterDataTable monsterData;
    
    public void Initialize(MonsterDataTable monsterData)
    {
        this.monsterData = monsterData;
        
        healthBar.SetValue(1.0f);
    }
}