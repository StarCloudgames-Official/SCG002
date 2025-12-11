using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class ClassTable
{
    public int id;
    public DataTableEnum.ClassType classType;
    public DataTableEnum.DrawType drawType;
    public float attackDamage;
    public float attackSpeed;
    public float criticalChance;
    public float criticalDamage;
    public string particleName;
}
