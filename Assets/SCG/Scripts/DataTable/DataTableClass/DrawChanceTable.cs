using System;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;

[MemoryPackable]
public partial class DrawChanceTable
{
    public int id;
    public DataTableEnum.DrawType drawType;
    public string drawChance;
}
