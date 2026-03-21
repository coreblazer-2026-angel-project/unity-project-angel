using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ManagerBase<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    private static readonly object locker = new object();
    private static bool isQuitting = false;

    public static T Instance {
        get {
            if (isQuitting) {
                Debug.LogWarning($"[Singleton] {typeof(T)} already destroyed. Returning null.");
                return null;
            }

            lock (locker) {
                if (instance == null) {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null) {
                        GameObject go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;
            }
        }
    }

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit() {
        isQuitting = true;
    }
}