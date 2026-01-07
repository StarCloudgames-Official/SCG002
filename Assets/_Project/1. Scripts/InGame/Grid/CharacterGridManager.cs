using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

public class CharacterGridManager : CachedMonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject characterGridPrefab;

    private List<CharacterGrid> spawnedCharacterGrids;
    private int gridCountX;
    private int gridCountY;
    private float gridWidth;
    private float gridHeight;
    private float offsetX;
    private float offsetY;

    public int TotalGridCount => spawnedCharacterGrids.Count;

    public async UniTask CreateGrid(int x, int y)
    {
        gridCountX = x;
        gridCountY = y;
        spawnedCharacterGrids = new List<CharacterGrid>(x * y);

        var handle = characterGridPrefab.LoadAssetAsync<GameObject>();
        await handle.Task;
        var prefab = handle.Result;

        var gridComponent = prefab.GetComponent<CharacterGrid>();
        gridWidth = gridComponent.GetXSize;
        gridHeight = gridComponent.GetYSize;

        offsetX = (x - 1) / 2f * gridWidth;
        offsetY = (y - 1) / 2f * gridHeight;

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

    public CharacterGrid GetCharacterGrid(int x, int y)
    {
        if (x < 0 || x >= gridCountX || y < 0 || y >= gridCountY)
            return null;

        var index = y * gridCountX + x;
        return spawnedCharacterGrids[index];
    }

    public CharacterGrid GetCharacterGridFromWorldPosition(Vector2 worldPosition)
    {
        var gridX = Mathf.RoundToInt((worldPosition.x + offsetX) / gridWidth);
        var gridY = Mathf.RoundToInt((worldPosition.y + offsetY) / gridHeight);

        return GetCharacterGrid(gridX, gridY);
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
