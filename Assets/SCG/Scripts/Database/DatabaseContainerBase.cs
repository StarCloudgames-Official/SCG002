using Cysharp.Threading.Tasks;

public abstract class DatabaseContainerBase
{
    protected abstract string PreferenceKey { get; }
    public bool IsDirty { get; private set; }
    protected void SetDirty(bool isDirty) => IsDirty = isDirty;
    public abstract void Initialize();
    public abstract void SaveToLocal();
    public abstract UniTask LoadLocalData();
}
