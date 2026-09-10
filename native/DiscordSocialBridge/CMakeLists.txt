cmake_minimum_required(VERSION 3.20)

project(
    DiscordSocialBridge
    LANGUAGES CXX
)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(
    DiscordSocialBridge
    SHARED

    DiscordSocialBridge.cpp
)

target_include_directories(
    DiscordSocialBridge
    PRIVATE

    "${CMAKE_CURRENT_SOURCE_DIR}/../sdk/include"
)

target_link_libraries(
    DiscordSocialBridge
    PRIVATE

    "${CMAKE_CURRENT_SOURCE_DIR}/../sdk/lib/discord_partner_sdk.lib"
)

set_target_properties(
    DiscordSocialBridge
    PROPERTIES

    OUTPUT_NAME "DiscordSocialBridge"
)