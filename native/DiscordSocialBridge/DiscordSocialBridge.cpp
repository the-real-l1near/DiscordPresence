#define DISCORDPP_IMPLEMENTATION

#include "../sdk/include/discordpp.h"
#include "DiscordSocialBridge.h"

#include <memory>
#include <optional>
#include <string>
#include <utility>

// ============================================================
// State
// ============================================================

namespace
{
    std::unique_ptr<discordpp::Client>
        g_client;

    bool g_initialized =
        false;

    // ========================================================
    // Helpers
    // ========================================================

    std::optional<std::string> ToOptionalString(
        const char* value
    )
    {
        if (value == nullptr ||
            value[0] == '\0')
        {
            return std::nullopt;
        }

        return std::string(
            value
        );
    }
}

// ============================================================
// C API
// ============================================================

extern "C"
{
    // ========================================================
    // Initialize
    // ========================================================

    bool DiscordSocial_Initialize(
        uint64_t applicationId
    )
    {
        if (g_initialized)
        {
            return true;
        }

        try
        {
            g_client =
                std::make_unique<discordpp::Client>();

            g_client->SetApplicationId(
                applicationId
            );

            g_initialized =
                true;

            return true;
        }
        catch (...)
        {
            g_client.reset();

            g_initialized =
                false;

            return false;
        }
    }

    // ========================================================
    // Set Activity
    // ========================================================

    bool DiscordSocial_SetActivity(
        const char* name,
        const char* details,
        const char* state,
        const char* largeImage,
        const char* largeText,
        int64_t startTimestamp
    )
    {
        if (!g_initialized ||
            !g_client)
        {
            return false;
        }

        try
        {
            discordpp::Activity activity;

            // ------------------------------------------------
            // Type
            // ------------------------------------------------

            activity.SetType(
                discordpp::ActivityTypes::Playing
            );

            // ------------------------------------------------
            // Name
            // ------------------------------------------------

            activity.SetName(
                name != nullptr
                    ? std::string(name)
                    : std::string()
            );

            // ------------------------------------------------
            // Details
            // ------------------------------------------------

            activity.SetDetails(
                ToOptionalString(
                    details
                )
            );

            // ------------------------------------------------
            // State
            // ------------------------------------------------

            activity.SetState(
                ToOptionalString(
                    state
                )
            );

            // ------------------------------------------------
            // Assets
            // ------------------------------------------------

            if (
                (
                    largeImage != nullptr &&
                    largeImage[0] != '\0'
                ) ||
                (
                    largeText != nullptr &&
                    largeText[0] != '\0'
                )
            )
            {
                discordpp::ActivityAssets assets;

                assets.SetLargeImage(
                    ToOptionalString(
                        largeImage
                    )
                );

                assets.SetLargeText(
                    ToOptionalString(
                        largeText
                    )
                );

                activity.SetAssets(
                    std::move(
                        assets
                    )
                );
            }

            // ------------------------------------------------
            // Timestamp
            // ------------------------------------------------

            if (startTimestamp > 0)
            {
                discordpp::ActivityTimestamps timestamps;

                timestamps.SetStart(
                    startTimestamp
                );

                activity.SetTimestamps(
                    std::move(
                        timestamps
                    )
                );
            }

            // ------------------------------------------------
            // Update Rich Presence
            // ------------------------------------------------

            g_client->UpdateRichPresence(
                std::move(
                    activity
                ),

                [](discordpp::ClientResult result)
                {
                    /*
                     * Async result hiện chưa expose
                     * sang C#.
                     */
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

    // ========================================================
    // Clear Activity
    // ========================================================

    void DiscordSocial_ClearActivity()
    {
        if (!g_initialized ||
            !g_client)
        {
            return;
        }

        try
        {
            /*
             * Dùng API clear chính thức của
             * Discord Social SDK.
             *
             * Không gửi một Activity rỗng bằng
             * UpdateRichPresence().
             */
            g_client->ClearRichPresence();
        }
        catch (...)
        {
        }
    }

    // ========================================================
    // Run Callbacks
    // ========================================================

    void DiscordSocial_RunCallbacks()
    {
        try
        {
            discordpp::RunCallbacks();
        }
        catch (...)
        {
        }
    }

    // ========================================================
    // Shutdown
    // ========================================================

    void DiscordSocial_Shutdown()
    {
        /*
         * Destroy native Client hoàn toàn.
         *
         * C# dùng cái này cho:
         *
         * - Discord restart recovery
         * - game override Suspend()
         * - application shutdown
         */
        g_client.reset();

        g_initialized =
            false;
    }
}