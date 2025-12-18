using UnityEngine;

public interface IUI
{
    public Awaitable PreOpen(object param = null);
    public Awaitable PreClose(object param = null);
    
    public Awaitable Open(object param = null);
    public Awaitable Close(object param = null);

    public void OnBackSpace();
}