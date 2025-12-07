using System;
using System.Collections.Generic;
using UnityEngine;

public class PSVSample : PooledGridScrollView<PSVSampleData, PSVSampleCell>
{
    private void Start()
    {
        var testDataList = new List<PSVSampleData>()
        {
            new PSVSampleData("1"),
            new PSVSampleData("2"),
            new PSVSampleData("3"),
            new PSVSampleData("4"),
            new PSVSampleData("5"),
            new PSVSampleData("6"),
            new PSVSampleData("7"),
            new PSVSampleData("8"),
            new PSVSampleData("9"),
            new PSVSampleData("10"),
            new PSVSampleData("11"),
            new PSVSampleData("12"),
            new PSVSampleData("13"),
            new PSVSampleData("14"),
            new PSVSampleData("15"),
            new PSVSampleData("16"),
            new PSVSampleData("17"),
            new PSVSampleData("18"),
            new PSVSampleData("19"),
        };

        SetData(
            testDataList,
            (data, index) => data != null && data.name == "16" ? new Vector2(150f, 300f) : Vector2.zero
        );
    }
}

public class PSVSampleData
{
    public string name;
    public PSVSampleData(string name)
    {
        this.name = name;
    }
}
