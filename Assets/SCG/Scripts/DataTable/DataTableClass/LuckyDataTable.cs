using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class LuckyDataTable
{
    public int id;
    public DataTableEnum.SpawnType spawnType;
    public int pricePoint;
    public float spawnChance;
    public string portraitName;
}
