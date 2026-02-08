using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class UniTaskExtensions
{
    public static async UniTask WaitUntilClose(this IUI ui)
    {
        var targetUI = UIManager.GetUI(ui);

        if (targetUI != null)
        {
            await UniTask.WaitWhile(() => UIManager.IsActivating(targetUI));
        }
    }

    public static async UniTask WaitAfterPlay(this Animator animator, string key)
    {
        animator.Play(key);
        await animator.WaitCurrentStateCompleteAsync();
    }

    public static async UniTask WaitCurrentStateCompleteAsync(this Animator animator, int layer = 0)
    {
        await UniTask.NextFrame();

        while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f)
        {
            await UniTask.NextFrame();
        }
    }

    public static async UniTask WhileAlive(IUI ui)
    {
        while (UIManager.IsActivating(ui))
        {
            await UniTask.NextFrame();
        }
    }

    public static async void Forget(this UniTask task)
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

    public static async void Forget<T>(this UniTask<T> task)
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
}
