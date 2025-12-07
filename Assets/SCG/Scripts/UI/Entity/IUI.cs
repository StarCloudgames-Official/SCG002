using UnityEngine;

public interface IUI
{
    public Awaitable PreOpen();
    public Awaitable PreClose();
    
    public Awaitable Open(object param = null);
    public Awaitable Close(object param = null);

    public void OnBackSpace();
}