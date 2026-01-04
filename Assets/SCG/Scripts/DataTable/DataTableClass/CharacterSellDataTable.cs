using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class CharacterSellDataTable
{
    public int id;
    public DataTableEnum.SpawnType spawnType;
    public int sellPrice;
}
