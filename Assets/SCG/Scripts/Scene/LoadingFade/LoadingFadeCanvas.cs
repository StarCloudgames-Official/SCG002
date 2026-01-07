using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingFadeCanvas : MonoBehaviour
{
    [SerializeField] private Animator fadeAnimator;

    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async UniTask StartFadeIn()
    {
        fadeAnimator.Play("FadeIn");
        await fadeAnimator.WaitCurrentStateCompleteAsync();
    }

    public async UniTask StartFadeOut()
    {
        fadeAnimator.Play("FadeOut");
        await fadeAnimator.WaitCurrentStateCompleteAsync();
    }
}
