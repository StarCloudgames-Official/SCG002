using System.Collections;
using UnityEngine;

public interface IWaveStarter
{
    public void StartStarter();
    public IEnumerator StartStarterCoroutine();
}

public class WaveStarterParam
{
    public int WaveNumber { get; set; }
    public string BossName { get; set; }
    public int MaxWave { get; set; }
}