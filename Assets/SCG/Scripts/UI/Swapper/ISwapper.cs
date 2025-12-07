public interface ISwapper
{
    public enum SwapType
    {
        None = 0,
        SwapType0 = 1,
        SwapType1 = 2,
        SwapType2 = 3,
        SwapType3 = 4,
        SwapType4 = 5,
        SwapType5 = 6,
        SwapType6 = 7,
        SwapType7 = 8,
        SwapType8 = 9,
        SwapType9 = 10,
    }

    public void Swap(SwapType swapType);
}