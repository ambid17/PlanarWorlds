using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Digger.Modules.Core.Sources.NativeCollections
{
    public static class Utils
    {
        public static void IncrementAt(NativeArray<int> bytes, int index)
        {
            unsafe {
                Interlocked.Increment(ref ((int*)bytes.GetUnsafePtr())[index]);
            }
        }

        public static void SetZeroAt(NativeArray<int> bytes, int index)
        {
            unsafe {
                Interlocked.Exchange(ref ((int*)bytes.GetUnsafePtr())[index], 0);
            }
        }
    }
}