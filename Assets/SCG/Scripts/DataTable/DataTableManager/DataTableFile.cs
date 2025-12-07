using System.Collections.Generic;
using MemoryPack;

[MemoryPackable]
public partial class DataTableFile
{
    public List<DataTableEntry> Tables { get; set; } = new();
}

[MemoryPackable]
public partial class DataTableEntry
{
    public string TableName { get; set; }
    public byte[] Payload { get; set; }
}