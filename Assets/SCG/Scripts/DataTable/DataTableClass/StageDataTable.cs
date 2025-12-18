using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class StageDataTable
{
    public int id;
    public int stage;
    public int floor;
    public int stageCount;
    public int[] bossStage;
    public int[] monsterIds;
    public int[] monsterCount;
}
