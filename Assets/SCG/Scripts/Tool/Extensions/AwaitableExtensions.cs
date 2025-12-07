using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine;

public static class AwaitableExtensions
{
    public static async void Forget(this Awaitable task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static async void Forget<T>(this Awaitable<T> task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    public static async Awaitable WaitUntilAsync(Func<bool> predicate)
    {
        while (!predicate())
        {
            await Awaitable.NextFrameAsync();
        }
    }

    public static async Awaitable WaitWhileAsync(Func<bool> predicate)
    {
        while (predicate())
        {
            await Awaitable.NextFrameAsync();
        }
    }
    
    public static async Awaitable WaitCurrentStateCompleteAsync(this Animator animator, int layer = 0)
    {
        await Awaitable.NextFrameAsync();

        while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
        { 
            await Awaitable.NextFrameAsync();
        }
    }
    
    public static async Awaitable ToAwaitable(this Tween tween)
    {
        if (tween == null || !tween.active) return;

        var completed = false;

        tween.OnComplete(() => completed = true);

        if (!tween.IsActive() || !tween.IsPlaying())
            completed = true;

        while (!completed)
        {
            await Awaitable.NextFrameAsync();
            if (!tween.IsActive())
                break;
        }
    }
    
    public static async Awaitable WhenAll(this IEnumerable<Awaitable> awaitables)
    {
        foreach (var awaitable in awaitables)
        {
            await awaitable;
        }
    }

    public static async Awaitable WhileAlive(IUI ui)
    {
        while (UIManager.IsActivating(ui))
        {
            await Awaitable.NextFrameAsync();
        }
    }
}