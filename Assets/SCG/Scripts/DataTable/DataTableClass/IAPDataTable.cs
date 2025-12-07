using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class IAPDataTable
{
    public int id;
    public string storeId_aos;
    public string storeId_ios;
    public int rewardGroupId;
    public bool consumable;
    public int purchasableCount;
    public int sortOrder;
}
