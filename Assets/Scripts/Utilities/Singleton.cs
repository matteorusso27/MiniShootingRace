using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        Instance = this as T;
    }
    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

public abstract class SingletonPersistent<TP> : Singleton<TP> where TP : MonoBehaviour
{
    protected override void Awake()
    {
        // Ensure only one instance, persist through different loads
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}
