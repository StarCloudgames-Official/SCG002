using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class UserBasicData
{
}

[AttributeExtensions.AutoRegisterDatabaseContainer]
public class UserBasicDatabaseContainer : DatabaseContainer<UserBasicData>
{
    protected override string PreferenceKey => "UserBasicDatabase";
}