using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class MonsterDataTable
{
    public int id;
    public float maxHealth;
    public float moveSpeed;
    public string animatorPath;
    public int dropCrystal;
    public string monsterName;
}
