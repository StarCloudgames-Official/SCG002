using UnityEngine;

public class PooledScrollCell : CachedMonoBehaviour
{
    public int Index { get; private set; } = -1;

    public virtual void SetIndex(int index)
    {
        Index = index;
    }

    public virtual void OnReused()
    {
    }
}

public class PooledScrollCell<TData> : PooledScrollCell
{
    public TData Data { get; private set; }

    public virtual void BindData(TData data, int index)
    {
        Data = data;
    }
}
