using System.Collections.Generic;
using UnityEngine;

public static class MonsterPath
{
    private static readonly Vector2[] MonsterPathArray = 
    {
        new(-2.0f, 2.5f),
        new(-2.0f, -2.5f),
        new(2.0f, -2.5f),
        new(2.0f, 2.5f),
    };
    
    public static Vector2 SpawnPosition = new(0, 2.5f);

    public static Vector2 GetSpawnPosition(int index)
    {
        return MonsterPathArray[index];
    }

    public static Vector2 GetNextSpawnPosition(int currentIndex, out int index)
    {
        index = currentIndex + 1 >= MonsterPathArray.Length ? 0 : currentIndex + 1;
        return GetSpawnPosition(index); 
    }
}