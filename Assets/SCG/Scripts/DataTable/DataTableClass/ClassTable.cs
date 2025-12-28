using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class ClassTable
{
    public int id;
    public DataTableEnum.ClassType classType;
    public DataTableEnum.SpawnType spawnType;
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public int attackMonsterCount;
    public float criticalChance;
    public float criticalDamage;
    public string particleName;
}
