using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class SpawnChanceTable
{
    public int id;
    public DataTableEnum.SpawnType spawnType;
    public float drawChance;
}
