using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class RewardGroupDataTable
{
    public int id;
    public int rewardGroupId;
    public DataTableEnum.AssetType rewardAssetType;
    public int rewardAmount;
}
