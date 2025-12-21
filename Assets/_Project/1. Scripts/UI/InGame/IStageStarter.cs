using System.Collections;
using UnityEngine;

public interface IStageStarter
{
    public void StartStarter();
    public IEnumerator StartStarterCoroutine();
}

public class StageStarterParam
{
    public int StageNumber { get; set; }
    public string BossName { get; set; }
    public int MaxStage { get; set; }
}