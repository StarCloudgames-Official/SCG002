using System.Collections.Generic;

public static class RewardDataExtension
{
    public static List<RewardData> UnionRewardDatas(this List<RewardData> rewardDatas)
    {
        var dict = new Dictionary<DataTableEnum.AssetType, int>();

        foreach (var rewardData in rewardDatas)
        {
            if (dict.TryGetValue(rewardData.AssetType, out var amount))
                dict[rewardData.AssetType] = amount + rewardData.Amount;
            else
                dict[rewardData.AssetType] = rewardData.Amount;
        }

        var result = new List<RewardData>(dict.Count);
        foreach (var pair in dict)
        {
            result.Add(new RewardData(pair.Key, pair.Value));
        }

        return result;
    }
}