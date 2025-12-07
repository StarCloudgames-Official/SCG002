using System;
using GUPS.AntiCheat.Protected.Storage.Prefs;
using UnityEngine;

public static class GameSettings
{
    public static bool CanHaptic
    {
        get => Convert.ToBoolean(ProtectedPlayerPrefs.GetInt("CanHaptic", 1));
        set => ProtectedPlayerPrefs.SetInt("CanHaptic", Convert.ToInt32(value));
    }

    public static bool CanSFX
    {
        get => Convert.ToBoolean(ProtectedPlayerPrefs.GetInt("CanSFX", 1));
        set
        {
            ProtectedPlayerPrefs.SetInt("CanSFX", Convert.ToInt32(value));
            SoundManager.SetSfxEnabled(false);
        }
    }

    public static bool CanBGM
    {
        get => Convert.ToBoolean(ProtectedPlayerPrefs.GetInt("CanBGM", 1));
        set
        {
            ProtectedPlayerPrefs.SetInt("CanBGM", Convert.ToInt32(value));
            SoundManager.SetBgmEnabled(true);
        }
    }
}