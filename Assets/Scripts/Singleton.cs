using UnityEngine;

/// <summary>
/// 基础单例类，提供简洁的单例实现
/// </summary>
/// <typeparam name="T">继承此类的类型</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance;

    protected virtual void Awake()
    {
        instance = this as T;
    }
}
