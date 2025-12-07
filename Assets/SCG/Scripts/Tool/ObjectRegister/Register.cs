using UnityEngine;

public class Register : MonoBehaviour
{
    [SerializeField] private ObjectRegister.RegisterType registerType;
    [SerializeField] private Component registerComponent;

    private void Awake()
    {
        if (registerType == ObjectRegister.RegisterType.None || !registerComponent) return;
        ObjectRegister.Register(registerType, registerComponent);
    }

    private void OnDestroy()
    {
        ObjectRegister.Unregister(registerType);
    }
}