using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LoadingFadeCanvas : MonoBehaviour
{
    [SerializeField] private Animator fadeAnimator;

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Awaitable StartFadeIn() //여기에 애니메이션 클립 이름 넣어서 LoadingFade(color = white) BlackFade(color = black) 해서 인게임 중에도 사용할 수 있게 변경?
    {
        fadeAnimator.Play("FadeIn");
        await fadeAnimator.WaitCurrentStateCompleteAsync();
    }

    public async Awaitable StartFadeOut()
    {
        fadeAnimator.Play("FadeOut");
        await fadeAnimator.WaitCurrentStateCompleteAsync();
    }
}