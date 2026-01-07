using Cysharp.Threading.Tasks;

public interface IUI
{
    public UniTask PreOpen(object param = null);
    public UniTask PreClose(object param = null);

    public UniTask Open(object param = null);
    public UniTask Close(object param = null);

    public void OnBackSpace();
}
