using System;

namespace MerkleTree.Types
{
    [Flags]
    public enum ChunkSize : int
    {
        SizeOf0 = 0,
        SizeOf1 = 1 << 0,
        SizeOf2 = 1 << 1,
        SizeOf4 = 1 << 2,
        SizeOf8 = 1 << 3,
        SizeOf16 = 1 << 4,
        SizeOf32 = 1 << 5,
        SizeOf64 = 1 << 6,
        SizeOf128 = 1 << 7,
        SizeOf256 = 1 << 8,
        SizeOf512 = 1 << 9,
        SizeOf1024 = 1 << 10,
        SizeOf2048 = 1 << 11,
        SizeOf4096 = 1 << 12,
        SizeOf8192 = 1 << 13,
        SizeOf16384 = 1 << 14,
        SizeOf32768 = 1 << 15,
        SizeOf65536 = 1 << 16,
        SizeOf131072 = 1 << 17,
        SizeOf262144 = 1 << 18,
        SizeOf524288 = 1 << 19,
        SizeOf1048576 = 1 << 20,
        SizeOf2097152 = 1 << 21,
        SizeOf4194304 = 1 << 22,
        SizeOf8388608 = 1 << 23,
        SizeOf16777216 = 1 << 24,
        SizeOf33554432 = 1 << 25,
        SizeOf67108864 = 1 << 26,
        SizeOf134217728 = 1 << 27,
        SizeOf268435456 = 1 << 28,
        SizeOf536870912 = 1 << 29,
        SizeOf1073741824 = 1 << 30,
    }
}
