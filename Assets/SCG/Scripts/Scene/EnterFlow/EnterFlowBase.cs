using Cysharp.Threading.Tasks;

public abstract class EnterFlowBase
{
    public abstract bool CanRunFlow();
    public abstract UniTask RunFlow();
}
