#define DISCORDPP_IMPLEMENTATION
#include "../sdk/include/discordpp.h"

#include "DiscordSocialBridge.h"

#include <memory>
#include <optional>
#include <string>
#include <utility>

namespace
{
    std::unique_ptr<discordpp::Client> g_client;
    bool g_initialized = false;

    std::optional<std::string> ToOptionalString(
        const char* value)
    {
        if (value == nullptr ||
            value[0] == '\0')
        {
            return std::nullopt;
        }

        return std::string(value);
    }
}

extern "C"
{
    bool DiscordSocial_Initialize(
        uint64_t applicationId)
    {
        if (g_initialized)
            return true;

        try
        {
            g_client =
                std::make_unique<discordpp::Client>();

            g_client->SetApplicationId(
                applicationId
            );

            g_initialized = true;

            return true;
        }
        catch (...)
        {
            g_client.reset();

            g_initialized = false;

            return false;
        }
    }

    bool DiscordSocial_SetActivity(
        const char* name,
        const char* details,
        const char* state,
        const char* largeImage,
        const char* largeText,
        int64_t startTimestamp)
    {
        if (!g_initialized ||
            !g_client)
        {
            return false;
        }

        try
        {
            discordpp::Activity activity;

            activity.SetType(
                discordpp::ActivityTypes::Playing
            );

            activity.SetName(
                name != nullptr
                    ? std::string(name)
                    : std::string()
            );

            activity.SetDetails(
                ToOptionalString(details)
            );

            activity.SetState(
                ToOptionalString(state)
            );

            // -------------------------------------------------
            // Assets
            // -------------------------------------------------

            if ((largeImage != nullptr &&
                 largeImage[0] != '\0') ||
                (largeText != nullptr &&
                 largeText[0] != '\0'))
            {
                discordpp::ActivityAssets assets;

                assets.SetLargeImage(
                    ToOptionalString(largeImage)
                );

                assets.SetLargeText(
                    ToOptionalString(largeText)
                );

                activity.SetAssets(
                    std::move(assets)
                );
            }

            // -------------------------------------------------
            // Timestamp
            //
            // 0 = disabled.
            // Discord Social SDK expects Unix time.
            // -------------------------------------------------

            if (startTimestamp > 0)
            {
                discordpp::ActivityTimestamps timestamps;

                timestamps.SetStart(
                    startTimestamp
                );

                activity.SetTimestamps(
                    std::move(timestamps)
                );
            }

            // -------------------------------------------------
            // Update
            // -------------------------------------------------

            g_client->UpdateRichPresence(
                std::move(activity),
                [](discordpp::ClientResult result)
                {
                    (void)result;
                }
            );

            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    void DiscordSocial_ClearActivity()
    {
        if (!g_initialized ||
            !g_client)
        {
            return;
        }

        try
        {
            discordpp::Activity activity;

            g_client->UpdateRichPresence(
                std::move(activity),
                [](discordpp::ClientResult result)
                {
                    (void)result;
                }
            );
        }
        catch (...)
        {
        }
    }

    void DiscordSocial_RunCallbacks()
    {
        discordpp::RunCallbacks();
    }

    void DiscordSocial_Shutdown()
    {
        g_client.reset();

        g_initialized = false;
    }
}