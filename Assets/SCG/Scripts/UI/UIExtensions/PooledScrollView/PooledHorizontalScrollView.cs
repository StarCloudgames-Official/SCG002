using UnityEngine;

public class PooledHorizontalScrollView<TData, TCell> : PooledSingleAxisScrollView<TData, TCell> where TCell : PooledScrollCell<TData>
{
    protected override void Awake()
    {
        axis = ScrollAxis.Horizontal;
        base.Awake();
        ConfigureScrollRect();
    }

    protected override void OnValidate()
    {
        axis = ScrollAxis.Horizontal;
        base.OnValidate();
    }
}
