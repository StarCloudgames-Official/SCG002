using UnityEngine;

public class MonsterHealth : CachedMonoBehaviour
{
    [SerializeField] private ObjectSlider healthBar;

    private MonsterDataTable monsterData;
    private float currentHealth;
    
    public void Initialize(MonsterDataTable monsterData)
    {
        this.monsterData = monsterData;
        currentHealth = monsterData.maxHealth;
        
        healthBar.SetValue(1.0f);
    }

    public bool GetDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
            currentHealth = 0;
        
        healthBar.SetValueAsync(currentHealth / monsterData.maxHealth, 0.1f).Forget();
        
        return currentHealth <= 0;
    }
    
    public bool IsDead() => currentHealth <= 0;
}