using UnityEngine;

public class PooledVerticalScrollView<TData, TCell> : PooledSingleAxisScrollView<TData, TCell> where TCell : PooledScrollCell<TData>
{
    protected override void Awake()
    {
        axis = ScrollAxis.Vertical;
        base.Awake();
        ConfigureScrollRect();
    }

    protected override void OnValidate()
    {
        axis = ScrollAxis.Vertical;
        base.OnValidate();
    }
}
