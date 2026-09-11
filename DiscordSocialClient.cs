using System.Runtime.InteropServices;

namespace DiscordPresence;

internal sealed class DiscordSocialClient : IDisposable
{
    private const ulong ApplicationId =
        1547227043429613668UL;

    private bool _initialized;
    private bool _disposed;

    public bool IsInitialized =>
        _initialized &&
        !_disposed;

    public bool ConsumeReadyEvent()
    {
        if (!IsInitialized)
        {
            return false;
        }

        return Native
            .DiscordSocial_ConsumeReadyEvent();
    }

    public bool Initialize()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this
        );

        if (_initialized)
            return true;

        _initialized =
            Native.DiscordSocial_Initialize(
                ApplicationId
            );

        return _initialized;
    }

    public bool SetPresence(
        string name,
        string? details,
        string? state,
        string? largeImage,
        string? largeText,
        DateTime? startTime)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this
        );

        if (!_initialized)
            return false;

        long startTimestamp =
            0;

        if (startTime.HasValue)
        {
            startTimestamp =
                new DateTimeOffset(
                    startTime.Value.ToUniversalTime()
                )
                .ToUnixTimeSeconds();
        }

        return Native.DiscordSocial_SetActivity(
            name,
            details,
            state,
            largeImage,
            largeText,
            startTimestamp
        );
    }

    public void ClearPresence()
    {
        if (!IsInitialized)
            return;

        Native.DiscordSocial_ClearActivity();
    }

    public void RunCallbacks()
    {
        if (!IsInitialized)
            return;

        Native.DiscordSocial_RunCallbacks();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_initialized)
        {
            Native.DiscordSocial_ClearActivity();
            Native.DiscordSocial_RunCallbacks();
            Native.DiscordSocial_Shutdown();

            _initialized =
                false;
        }

        _disposed =
            true;
    }

    private static class Native
    {
        private const string DllName =
            "DiscordSocialBridge.dll";

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool
            DiscordSocial_ConsumeReadyEvent();

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool DiscordSocial_Initialize(
            ulong applicationId
        );

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool DiscordSocial_SetActivity(
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string name,

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string? details,

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string? state,

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string? largeImage,

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string? largeText,

            long startTimestamp
        );

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void
            DiscordSocial_ClearActivity();

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void
            DiscordSocial_RunCallbacks();

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void
            DiscordSocial_Shutdown();
    }
}