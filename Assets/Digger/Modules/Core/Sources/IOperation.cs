using Unity.Jobs;

namespace Digger.Modules.Core.Sources
{
    public interface IOperation<T> where T : struct, IJobParallelFor
    {
        ModificationArea GetAreaToModify(DiggerSystem digger);
        T Do(VoxelChunk chunk);
        void Complete(T job, VoxelChunk chunk);
    }
}