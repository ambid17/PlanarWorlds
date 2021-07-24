using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected StaticMonoBehaviour() { }

    protected static T ms_instance;

    public static T GetInstance()
    {
        if (ms_instance == null)
        {
            ms_instance = FindObjectOfType<T>();
            if (ms_instance == null)
            {
                GameObject singleton = new GameObject();
                ms_instance = singleton.AddComponent<T>();
                singleton.name = "[singleton] " + typeof(T).ToString();
            }
        }
        return ms_instance;
    }

    protected virtual void Awake()
    {
        if (ms_instance != null && ms_instance != gameObject)
        {
            Destroy(gameObject);
        }
        else
        {
            ms_instance = (T)(object)this;
        }
        Initialize();
    }
    /// <summary>
    /// Initialization override
    /// </summary>
    public virtual void Initialize() { }

}
