using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public struct Awaiter
{
    private int refCount;
    
    public void AddReference()
    {
        refCount++;
    }

    public void ReleaseRef()
    {
        refCount--;
    }

    public bool IsCompleted()
    {
        return refCount == 0;
    }

    public void OnCompleted(Action onComplete)
    {
        if (onComplete != null)
        {
            Task.Run(onComplete);
        }
    }
}
