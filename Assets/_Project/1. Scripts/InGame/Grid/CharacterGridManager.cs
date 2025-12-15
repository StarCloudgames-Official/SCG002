using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class CharacterGridManager : CachedMonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject characterGridPrefab;
    
    private List<CharacterGrid> spawnedCharacterGrids;

    public async Awaitable CreateGrid(int x, int y)
    {
        spawnedCharacterGrids = new List<CharacterGrid>(x * y);
        
        var handle = characterGridPrefab.LoadAssetAsync<GameObject>();
        await handle.Task;
        var prefab = handle.Result;
        
        var gridComponent = prefab.GetComponent<CharacterGrid>();
        var gridWidth = gridComponent.GetXSize;
        var gridHeight = gridComponent.GetYSize;
        
        var offsetX = (x - 1) / 2f * gridWidth;
        var offsetY = (y - 1) / 2f * gridHeight;
        
        for (var row = 0; row < y; row++)
        {
            for (var col = 0; col < x; col++)
            {
                var posX = col * gridWidth - offsetX;
                var posY = row * gridHeight - offsetY;
                
                var gridObj = Instantiate(prefab, new Vector3(posX, posY, 0), Quaternion.identity, transform);
                var grid = gridObj.GetComponent<CharacterGrid>();
                spawnedCharacterGrids.Add(grid);
            }
        }
    }

    public CharacterGrid GetEmptyGrid()
    {
        foreach (var characterBehaviour in spawnedCharacterGrids)
        {
            if(characterBehaviour.IsEmpty)
                return characterBehaviour;
        }
        
        return null;
    }

    public CharacterGrid GetRandomEmptyGrid()
    {
        var emptyGrids = ListPool<CharacterGrid>.Get();
        
        foreach (var grid in spawnedCharacterGrids)
        {
            if (grid.IsEmpty)
                emptyGrids.Add(grid);
        }
        
        if (emptyGrids.Count == 0)
        {
            ListPool<CharacterGrid>.Release(emptyGrids);
            return null;
        }
        
        var result = emptyGrids[Random.Range(0, emptyGrids.Count)];
        ListPool<CharacterGrid>.Release(emptyGrids);
        return result;
    }
}