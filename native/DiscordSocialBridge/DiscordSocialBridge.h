#pragma once

#include <stdint.h>

#ifdef _WIN32
#define DSP_EXPORT __declspec(dllexport)
#else
#define DSP_EXPORT
#endif

extern "C"
{
    DSP_EXPORT bool DiscordSocial_Initialize(
        uint64_t applicationId
    );

    DSP_EXPORT bool DiscordSocial_SetActivity(
        const char* name,
        const char* details,
        const char* state,
        const char* largeImage,
        const char* largeText,
        int64_t startTimestamp
    );

    DSP_EXPORT void DiscordSocial_ClearActivity();

    DSP_EXPORT void DiscordSocial_RunCallbacks();

    DSP_EXPORT bool DiscordSocial_ConsumeReadyEvent();

    DSP_EXPORT void DiscordSocial_Shutdown();
}