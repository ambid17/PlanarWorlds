using Unity.Mathematics;

namespace Digger.Modules.Core.Sources
{
    public struct Voxel
    {
        /// <summary>
        /// This is the voxel SDF (signed distance field) value. Used to create mesh vertices at the right place.
        /// Negative value means the voxel is inside the volume. Positive value means the voxel is outside the volume.
        /// Zero value means the voxel is exactly on the volume's surface.
        /// </summary>
        public float Value;

        /// <summary>
        /// This 32-bits uint stores many properties about this voxel:
        /// 
        /// - bites 1 to 5 store first texture index (5 bites, so 32 possible values)
        /// - bites 6 to 10 store second texture index (5 bites, so 32 possible values)
        /// - bites 11 to 16 store lerp (weight) value between the two textures (6 bites, so 64 possible values)
        /// - bites 17 to 19 store weight of MicroSplat wetness (3 bites, so only 8 possible values)
        /// - bites 20 to 22 store weight of MicroSplat puddles (3 bites, so only 8 possible values)
        /// - bites 23 to 25 store weight of MicroSplat streams (3 bites, so only 8 possible values)
        /// - bites 26 to 28 store weight of MicroSplat lava (3 bites, so only 8 possible values)
        /// - bites 29 to 32 store specific information:
        ///        0 => not altered, no visual mesh generated
        ///        1 => not altered but visual mesh must be generated on terrain surface
        ///        2 => altered, near terrain surface, below terrain surface
        ///        3 => altered, near terrain surface, above terrain surface
        ///        4 => altered, not near terrain surface, below terrain surface
        ///        5 => altered, not near terrain surface, above terrain surface
        ///
        /// </summary>
        private uint properties;

        public uint FirstTextureIndex {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0001_1111) << 0;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1110_0000) << 0;
            }
            get => (properties & 0b0000_0000_0000_0000_0000_0000_0001_1111) >> 0;
        }

        public uint SecondTextureIndex {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0001_1111) << 5;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1110_0000) << 5 |
                              0b0000_0000_0000_0000_0000_0000_0001_1111;
            }
            get => (properties & 0b0000_0000_0000_0000_0000_0011_1110_0000) >> 5;
        }

        private uint TextureLerp {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0011_1111) << 10;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1100_0000) << 10 |
                              0b0000_0000_0000_0000_0000_0011_1111_1111;
            }
            get => (properties & 0b0000_0000_0000_0000_1111_1100_0000_0000) >> 10;
        }

        private uint WetnessWeight {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0000_0111) << 16;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1111_1000) << 16 |
                              0b0000_0000_0000_0000_1111_1111_1111_1111;
            }
            get => (properties & 0b0000_0000_0000_0111_0000_0000_0000_0000) >> 16;
        }

        private uint PuddlesWeight {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0000_0111) << 19;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1111_1000) << 19 |
                              0b0000_0000_0000_0111_1111_1111_1111_1111;
            }
            get => (properties & 0b0000_0000_0011_1000_0000_0000_0000_0000) >> 19;
        }

        private uint StreamsWeight {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0000_0111) << 22;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1111_1000) << 22 |
                              0b0000_0000_0011_1111_1111_1111_1111_1111;
            }
            get => (properties & 0b0000_0001_1100_0000_0000_0000_0000_0000) >> 22;
        }

        private uint LavaWeight {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0000_0111) << 25;
                properties &= (value | 0b1111_1111_1111_1111_1111_1111_1111_1000) << 25 |
                              0b0000_0001_1111_1111_1111_1111_1111_1111;
            }
            get => (properties & 0b0000_1110_0000_0000_0000_0000_0000_0000) >> 25;
        }

        public uint Alteration {
            set {
                properties |= (value & 0b0000_0000_0000_0000_0000_0000_0000_1111) << 28;
                properties &= ((value | 0b1111_1111_1111_1111_1111_1111_1111_0000) << 28) |
                              0b0000_1111_1111_1111_1111_1111_1111_1111;
            }
            get => (properties & 0b1111_0000_0000_0000_0000_0000_0000_0000) >> 28;
        }

        public float NormalizedTextureLerp {
            set => TextureLerp = (uint) (math.clamp(value, 0f, 1f) * 63f);
            get => TextureLerp / 63f;
        }

        public float NormalizedWetnessWeight {
            set => WetnessWeight = (uint) (math.clamp(value, 0f, 1f) * 7f);
            get => WetnessWeight / 7f;
        }

        public float NormalizedPuddlesWeight {
            set => PuddlesWeight = (uint) (math.clamp(value, 0f, 1f) * 7f);
            get => PuddlesWeight / 7f;
        }

        public float NormalizedStreamsWeight {
            set => StreamsWeight = (uint) (math.clamp(value, 0f, 1f) * 7f);
            get => StreamsWeight / 7f;
        }

        public float NormalizedLavaWeight {
            set => LavaWeight = (uint) (math.clamp(value, 0f, 1f) * 7f);
            get => LavaWeight / 7f;
        }

        public Voxel(float value)
        {
            Value = value;
            properties = 0;
        }

        public bool IsInside => Value < 0;
        
        public bool IsInsideInclusive => Value < 0.0001f;

        public bool IsAlteredNearBelowSurface => Alteration == NearBelowSurface;

        public bool IsAlteredNearAboveSurface => Alteration == NearAboveSurface;

        public bool IsAlteredFarOrNearSurface => Alteration >= 2;
        public bool IsAlteredFarSurface => Alteration >= 4;

        public bool IsUnalteredOrOnSurface => Alteration <= 1;

        ///        0 => not altered, no visual mesh generated
        ///        1 => not altered but visual mesh must be generated on terrain surface
        ///        2 => altered, near terrain surface, below terrain surface
        ///        3 => altered, near terrain surface, above terrain surface
        ///        4 => altered, not near terrain surface, below terrain surface
        ///        5 => altered, not near terrain surface, above terrain surface
        ///        6 => altered, but a hole is cut
        public const uint Unaltered = 0;

        public const uint OnSurface = 1;
        public const uint NearBelowSurface = 2;
        public const uint NearAboveSurface = 3;
        public const uint FarBelowSurface = 4;
        public const uint FarAboveSurface = 5;
        public const uint Hole = 6;

        public void AddTexture(uint textureIndex, float intensity)
        {
            if (textureIndex == FirstTextureIndex) {
                NormalizedTextureLerp -= intensity;
            } else if (textureIndex == SecondTextureIndex) {
                NormalizedTextureLerp += intensity;
            } else {
                if (NormalizedTextureLerp < 0.5f) {
                    SecondTextureIndex = textureIndex;
                    NormalizedTextureLerp = intensity;
                } else {
                    FirstTextureIndex = textureIndex;
                    NormalizedTextureLerp = 1f - intensity;
                }
            }
        }

        public void SetTexture(uint textureIndex, float intensity)
        {
            if (textureIndex == FirstTextureIndex) {
                NormalizedTextureLerp = intensity;
            } else if (textureIndex == SecondTextureIndex) {
                NormalizedTextureLerp = intensity;
            } else {
                if (NormalizedTextureLerp < 0.5f) {
                    SecondTextureIndex = textureIndex;
                    NormalizedTextureLerp = intensity;
                } else {
                    FirstTextureIndex = textureIndex;
                    NormalizedTextureLerp = 1f - intensity;
                }
            }
        }
    }
}