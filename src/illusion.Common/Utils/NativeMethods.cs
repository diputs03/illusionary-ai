using System;
using System.Runtime.InteropServices;

namespace illusion.Common.Utils;

internal static class NativeMethods
{
    private const string NativeDllName = "illusion.Common.Native.dll";

    [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool illusion_crypto_sha256(
        [In] byte[] inputData,
        UIntPtr inputLength,
        [Out] byte[] outputHash,
        UIntPtr outputHashSize
    );

    [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool illusion_crypto_verify_signature(
        [In] byte[] moduleData,
        UIntPtr moduleLength,
        [In] byte[] signature,
        UIntPtr signatureLength,
        [In] byte[] publicKey,
        UIntPtr publicKeyLength
    );
}