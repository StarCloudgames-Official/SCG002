using UnityEngine;
using UnityEngine.Rendering;

public abstract class SwapperBehaviour : CachedMonoBehaviour, ISwapper
{
    public abstract void Swap(ISwapper.SwapType swapType);
}

public abstract class SwapperBase<T> : SwapperBehaviour
{
    [SerializeField] protected SerializedDictionary<ISwapper.SwapType, T> swapDictionary;
}