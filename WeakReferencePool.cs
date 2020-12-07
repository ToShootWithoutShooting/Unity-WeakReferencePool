using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 一个弱引用的内存池。不会造成内存泄漏。
/// </summary>
/// <typeparam name="T"></typeparam>
public class CachePool<T> where T : class
{
    public CachePool(bool isWeak = false)
    {
        mWeakRef = isWeak;
    }
    private bool mWeakRef; //是否弱引用，弱引用用mCache弱引用字典，否则用普通字典mCache2
    private Dictionary<string, WeakReference<T>> mCache = new Dictionary<string, WeakReference<T>>();
    private Dictionary<string, T> mCache2 = new Dictionary<string, T>();
    public T Get(string name)
    {
        if (name == null) return null;
        if (!mWeakRef)
        {
            mCache2.TryGetValue(name, out var result);
            return result;
        }
        if (mCache.TryGetValue(name, out var val))
        {
            val.TryGetTarget(out var tv);
            return tv;
        }
        return null;
    }
    public bool Update(string name, T val)
    {
        if (name == null || val == null)
            return false;

        if (!mWeakRef)
        {
            mCache2[name] = val;
            return true;
        }

        if (mCache.TryGetValue(name, out var refer))
        {
            refer.SetTarget(val);
        }
        else
        {
            mCache[name] = new WeakReference<T>(val);
        }

        return true;
    }
    public void Clear()
    {
        mCache.Clear();
        mCache2.Clear();
    }
}

public class ObjectLifeCycle
{
    private float mUpdateLife;
    public bool mIsPool;
    public void UpdateLife()
    {
        mUpdateLife = Time.time;
    }
    public float GetUpdateLife()
    {
        return mUpdateLife;
    }
}

public class CachePoolLifeCycle<T> where T : ObjectLifeCycle
{
    private LinkedList<T> mCache = new LinkedList<T>();
    float mLastUpdateTime;
    public void UpdateLife()
    {
        if (mLastUpdateTime == 0)
        {
            mLastUpdateTime = Time.time;
        }
        if (Time.time - mLastUpdateTime < 60) // 每60秒检查
            return;
        mLastUpdateTime = Time.time;
        var delTime = Time.time - 60 * 2;     // 2分钟没使用过的回收

        var node = mCache.First;
        while (node != mCache.Last)
        {
            var removenode = node;
            node = node.Next;
            if (removenode.Value.GetUpdateLife() > 0 && removenode.Value.GetUpdateLife() < delTime)
            {
                removenode.Value.mIsPool = false;
                mCache.Remove(removenode);
            }
        }

    }
    public void Put(T obj)
    {
        if (obj.mIsPool)
            return;
        obj.mIsPool = true;
        mCache.AddLast(obj);
    }
}