using System;
using System.Collections.Generic;

namespace SCG.GoogleSheets
{
    [Serializable]
    public class SpreadSheetMeta
    {
        public SheetInfo[] sheets;
    }

    [Serializable]
    public class SheetInfo
    {
        public SheetProperties properties;
    }

    [Serializable]
    public class SheetProperties
    {
        public string title;
    }

    [Serializable]
    public class SheetResponse
    {
        public List<List<string>> values;
    }
}
