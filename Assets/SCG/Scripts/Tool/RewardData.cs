public class RewardData
{
    public DataTableEnum.AssetType AssetType;
    public int Amount;

    public RewardData(DataTableEnum.AssetType assetType, int amount)
    {
        AssetType = assetType;
        Amount = amount;
    }
}