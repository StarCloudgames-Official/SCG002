using System;
using System.Collections.Generic;
using MemoryPack;

public partial class IAPDataTable
{
    [MemoryPackIgnore]
    private IReadOnlyList<RewardGroupDataTable> rewardGroupData;

    [MemoryPackIgnore]
    public IReadOnlyList<RewardGroupDataTable> RewardGroupData
    {
        get
        {
            if (rewardGroupData == null)
            {
                var manager = DataTableManager.Instance;
                if (manager != null)
                    rewardGroupData = manager.GetTargetRewardGroupRewards(rewardGroupId);
            }

            return rewardGroupData ?? Array.Empty<RewardGroupDataTable>();
        }
    }
}
