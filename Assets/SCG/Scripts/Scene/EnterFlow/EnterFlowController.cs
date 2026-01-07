using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class EnterFlowController
{
    private readonly List<EnterFlowBase> enterFlows = new();

    public void AddEnterFlow(EnterFlowBase enterFlow)
    {
        enterFlows.Add(enterFlow);
    }

    public async UniTask RunFlow()
    {
        foreach (var enterFlow in enterFlows)
        {
            if(enterFlow.CanRunFlow())
                await enterFlow.RunFlow();
        }
    }
}
