using System;
using UnityEditorInternal;

public static class StringExtensions
{
    public static string DateTimeToStringToMMSS(this DateTime dateTime)
    {
        return $"{dateTime.Minute:D2}:{dateTime.Second:D2}";
    }

    public static string DateTimeToStringToHHMMSS(this DateTime dateTime)
    {
        return $"{dateTime.Hour}:{dateTime.Minute:D2}:{dateTime.Second:D2}";
    }
}