using System;

public static partial class DataTableEnum
{
    public enum AssetType
    {
        Gold = 0,
    }

    public enum SpawnType
    {
        None = 0,
        Normal = 1,
        Advaned = 2,
        Elite = 3,
        Master = 4,
        Epic = 5,
        Legendary = 6,
        Godlike = 7,
    }

    public enum InGameStatType
    {
        None = 0,
        BaseDamage = 1,
        BaseAttackSpeed = 2,
        BaseCriticalRatio = 3,
        BaseCriticalDamage = 4,
    }

    public enum ClassType
    {
        None = 0,
        Warrior = 1,
        Mage = 2,
        Rogue = 3,
    }

}
