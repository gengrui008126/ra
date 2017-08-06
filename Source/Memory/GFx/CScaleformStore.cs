namespace RAGENativeUI.Memory.GFx
{
    using System;
    using System.Runtime.InteropServices;

    using Rage;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CScaleformStore
    {
        [FieldOffset(0x0038)] public CPool Pool;

        public CScaleformDef* GetPoolItem(int index)
        {
            return (CScaleformDef*)Pool.Get(unchecked((uint)index));
        }

        private static CScaleformStore* instance;
        public static CScaleformStore* GetInstance()
        {
            if (instance == null)
            {
                IntPtr address = Game.FindPattern("48 8D 0D ?? ?? ?? ?? 8B D3 E8 ?? ?? ?? ?? 84 C0 74 18");
                address = address + *(int*)(address + 3) + 7;
                instance = (CScaleformStore*)address;
            }

            return instance;
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = 72)]
    internal unsafe struct ScaleformData1
    {
        [FieldOffset(0x0000)] public short ScaleformIndex; // in ScaleformData2 array


        private static CArray_ScaleformData1* arrayInstance;
        public static CArray_ScaleformData1* GetArrayInstance()
        {
            if (arrayInstance == null)
            {
                // TODO: ScaleformData1.GetArrayInstance use FindPattern
                arrayInstance = (CArray_ScaleformData1*)(System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress + 0x244EAF0);
            }

            return arrayInstance;
        }


        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct CArray_ScaleformData1
        {
            private ScaleformData1 start;

            public ScaleformData1* Get(int index)
            {
                fixed (ScaleformData1* array = &start)
                {
                    return &array[index];
                }
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 480)]
    internal unsafe struct ScaleformData2
    {
        [FieldOffset(0x00B0)] public int ScaleformStorePoolIndex;


        private static CSimpleArray_ScaleformData2* arrayInstance;
        public static CSimpleArray_ScaleformData2* GetArrayInstance()
        {
            if (arrayInstance == null)
            {
                // TODO: ScaleformData2.GetArrayInstance use FindPattern
                arrayInstance = (CSimpleArray_ScaleformData2*)(System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress + 0x1F91E70);
            }

            return arrayInstance;
        }


        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct CSimpleArray_ScaleformData2
        {
            public ScaleformData2* Offset;
            public short Count;
            public short Size;

            public ScaleformData2* Get(short index)
            {
                if (index >= Size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"The size of this {nameof(CSimpleArray_ScaleformData2)} is {Size}, the index {index} is out of range.");
                }

                return &Offset[index];
            }
        }
    }
}

