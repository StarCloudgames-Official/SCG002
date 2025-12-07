using UnityEngine;

public abstract class EnterFlowBase
{
    public abstract bool CanRunFlow();
    public abstract Awaitable RunFlow();
}