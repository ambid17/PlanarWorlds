using System;
using Unity.Mathematics;

namespace Digger.Modules.Core.Sources
{
    public struct ModificationParameters
    {
        public float3 Position;
        public BrushType Brush;
        public ActionType Action;
        public int TextureIndex;
        public float Opacity;
        public float3 Size;
        public bool StalagmiteUpsideDown;
        public bool OpacityIsTarget;
        public Action Callback;

        public static ModificationParameters Default => new ModificationParameters
        {
            Brush = BrushType.Sphere,
            Position = default,
            Action = ActionType.Dig,
            TextureIndex = 0,
            Opacity = 0.5f,
            Size = 4f,
            StalagmiteUpsideDown = false,
            OpacityIsTarget = false,
            Callback = null
        };
    }
}