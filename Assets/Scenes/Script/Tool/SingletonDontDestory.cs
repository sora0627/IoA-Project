using UnityEngine;
public class SingletonDontDestroy<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance;
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)FindObjectOfType(typeof(T));
            DontDestroyOnLoad(gameObject); // í«â¡ÅIÅI
        }
        else
            Destroy(gameObject);
    }
}