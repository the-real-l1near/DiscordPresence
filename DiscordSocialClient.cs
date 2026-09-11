using System.Runtime.InteropServices;

namespace DiscordPresence;

internal sealed class DiscordSocialClient : IDisposable
{
    // =========================================================
    // Discord application
    // =========================================================

    private const ulong ApplicationId =
        1547227043429613668UL;

    // =========================================================
    // State
    // =========================================================

    private bool _initialized;

    private bool _disposed;

    // =========================================================
    // Properties
    // =========================================================

    public bool IsInitialized =>
        _initialized &&
        !_disposed;

    // =========================================================
    // Initialize
    // =========================================================

    public bool Initialize()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this
        );

        if (_initialized)
        {
            return true;
        }

        _initialized =
            Native.DiscordSocial_Initialize(
                ApplicationId
            );

        return _initialized;
    }

    // =========================================================
    // Reinitialize
    // =========================================================

    public bool Reinitialize()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this
        );

        /*
         * Client cũ có thể được tạo khi Discord chưa chạy
         * hoặc đã mất local connection sau Discord restart.
         *
         * Tạo lại native client hoàn toàn.
         */
        if (_initialized)
        {
            Native.DiscordSocial_Shutdown();

            _initialized =
                false;
        }

        _initialized =
            Native.DiscordSocial_Initialize(
                ApplicationId
            );

        return _initialized;
    }

    // =========================================================
    // Suspend
    // =========================================================

    public void Suspend()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this
        );

        if (!_initialized)
        {
            return;
        }

        /*
         * Clear Rich Presence trước.
         */
        Native.DiscordSocial_ClearActivity();

        /*
         * Pump một lượt trước khi shutdown.
         */
        Native.DiscordSocial_RunCallbacks();

        /*
         * Ngắt hoàn toàn native Social SDK client.
         *
         * Dùng khi game đang foreground để app mình
         * không giữ activity Coding.
         */
        Native.DiscordSocial_Shutdown();

        _initialized =
            false;
    }

    // =========================================================
    // Presence
    // =========================================================

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
        {
            return false;
        }

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

    // =========================================================
    // Clear
    // =========================================================

    public void ClearPresence()
    {
        if (!IsInitialized)
        {
            return;
        }

        Native.DiscordSocial_ClearActivity();
    }

    // =========================================================
    // Callbacks
    // =========================================================

    public void RunCallbacks()
    {
        if (!IsInitialized)
        {
            return;
        }

        Native.DiscordSocial_RunCallbacks();
    }

    // =========================================================
    // Dispose
    // =========================================================

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

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

    // =========================================================
    // Native bridge
    // =========================================================

    private static class Native
    {
        private const string DllName =
            "DiscordSocialBridge.dll";

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl
        )]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool DiscordSocial_Initialize(
            ulong applicationId
        );

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl
        )]
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
            CallingConvention = CallingConvention.Cdecl
        )]
        internal static extern void DiscordSocial_ClearActivity();

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl
        )]
        internal static extern void DiscordSocial_RunCallbacks();

        [DllImport(
            DllName,
            CallingConvention = CallingConvention.Cdecl
        )]
        internal static extern void DiscordSocial_Shutdown();
    }
}