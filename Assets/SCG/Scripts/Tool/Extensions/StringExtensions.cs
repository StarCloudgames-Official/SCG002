using System;
using Cysharp.Text;

public static class StringExtensions
{
    public static string DateTimeToStringToMMSS(this DateTime dateTime)
    {
        return ZString.Format("{0:D2}:{1:D2}", dateTime.Minute, dateTime.Second);
    }

    public static string DateTimeToStringToHHMMSS(this DateTime dateTime)
    {
        return ZString.Format("{0}:{1:D2}:{2:D2}", dateTime.Hour, dateTime.Minute, dateTime.Second);
    }
}