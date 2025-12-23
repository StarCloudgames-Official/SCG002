using System.Collections.Generic;
using UnityEngine;

public static class MonsterPath
{
    private static readonly Dictionary<int, Vector2> MonsterPathArray = new()
    {
        { 0, new Vector2(-2.0f, 2.5f)},
        { 1, new Vector2(-2.0f, -2.5f)},
        { 2, new Vector2(2.0f, -2.5f)},
        { 3, new Vector2(2.0f, 2.5f)},
    };
    
    public static Vector2 SpawnPosition = new(0, 2.5f);

    public static Vector2 GetSpawnPosition(int index)
    {
        return MonsterPathArray[index];
    }

    public static Vector2 GetNextSpawnPosition(int currentIndex, out int index)
    {
        index = currentIndex + 1 >= MonsterPathArray.Count ? 0 : currentIndex + 1;
        return GetSpawnPosition(index); 
    }
}