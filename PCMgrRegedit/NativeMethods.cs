using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PCMgrRegedit
{
    internal static class NativeMethods
    {
#if _X64_
        public const string COREDLLNAME = "PCMgr64.dll";
#else
        public const string COREDLLNAME = "PCMgr32.dll";
#endif

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public delegate bool ENUMKEYVALECALLBACK(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string valueName, uint dataType, uint dataSize, [MarshalAs(UnmanagedType.LPWStr)]string dataSample, uint index, uint allCount);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public delegate bool ENUMKEYSCALLBACK(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string childKeyName, bool hasChild, uint index, uint allCount);


        public const uint REG_SZ = 1u;
        public const uint REG_EXPAND_SZ = 2u;
        public const uint REG_BINARY = 3u;
        public const uint REG_DWORD = 4u;
        public const uint REG_MULTI_SZ = 7u;
        public const uint REG_QWORD = 11u;

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MFM_GetFolderIcon();
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MFM_GetMyComputerIcon();
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern string MFM_GetMyComputerName();

        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MREG_ROOTKEYToStr(IntPtr hRootKey);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MREG_RegTypeToStr(uint regType);
        public static string MREG_RegTypeToIcon(uint regType)
        {
            if (regType == 1 || regType == 2 || regType == 6 || regType == 7 || regType == 8 || regType == 9 || regType == 10)
                return "ItemText";
            else if (regType == 3 || regType == 4 || regType == 5 || regType == 11)
                return "ItemBinary";
            return "";
        }
        public static bool MREG_RegTypeIsSz(uint regType)
        {
            if (regType == 1 || regType == 2 || regType == 6 || regType == 7 || regType == 8 || regType == 9 || regType == 10)
                return true;
            return false;
        }

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MREG_GetROOTKEY(int i);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_EnumKeyVaules(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr callBack);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_EnumKeys(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr callBack);


        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_DeleteKey(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_RenameKey(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string newName);

        [return: MarshalAs(UnmanagedType.LPWStr)]
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string MREG_GetLastErrString();       
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MREG_GetLastErr();

        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_CreateSubKey(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string newKeyName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_CreateValue(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string newValueName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_DeleteKeyValue(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string valueName);
        [DllImport(COREDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool MREG_RenameValue(IntPtr hRootKey, [MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)]string valueName, [MarshalAs(UnmanagedType.LPWStr)]string newValueName);

        //MREG_CreateSubKey
        //MREG_CreateValue
        //MREG_RenameValue
    }
}
