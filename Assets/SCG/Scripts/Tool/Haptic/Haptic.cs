using System;
using Solo.MOST_IN_ONE;
using UnityEngine;

public static class Haptic
{
    public static void Play(MOST_HapticFeedback.HapticTypes hapticType)
    {
        if (!GameSettings.CanHaptic) return;
        MOST_HapticFeedback.Generate(hapticType);
    }

    public static void PlayWithCoolDown(MOST_HapticFeedback.HapticTypes hapticType, float coolDown = 0.1f)
    {
        if (!GameSettings.CanHaptic) return;
        MOST_HapticFeedback.GenerateWithCooldown(hapticType, coolDown);
    }
}