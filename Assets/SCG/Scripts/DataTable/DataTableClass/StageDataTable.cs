using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class StageDataTable
{
    public int id;
    public int stage;
    public DataTableEnum.Difficulty difficulty;
    public int waveCount;
    public int[] bossWave;
    public int[] monsterIds;
    public int[] monsterCount;
    public float[] spawnDelay;
    public float[] waveTimer;
    public int[] waveClearGold;
    public int stageClearRewardGroupId;
}
