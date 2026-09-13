# Valheim Discord Connect POC

This is a server-side BepInEx plugin for a Linux headless Valheim server. It logs the current Valheim players and posts the same list to a Discord channel through a webhook on a timer.

## Discord setup

1. Create a webhook for the target channel in Discord: **Edit Channel > Integrations > Webhooks > New Webhook**.
2. Copy the webhook URL. Treat it like a password and never commit it.

The webhook URL shared during setup must be revoked and regenerated because it is now exposed. Do not put the replacement URL in this repository or in chat.

## Build

The project targets .NET Framework 4.7.2 and can be built normally with the SDK installed. The default output directory is the standard .NET build folder in the project:

```bash
dotnet restore
dotnet build -c Release
```

This produces the DLL in:

```text
bin/Release/net472/BepInExDiscordConnect.dll
```

When building against a real Valheim server install, set `ValheimPath` if needed:

```bash
dotnet build -c Release -p:ValheimPath=/opt/valheim
```

The expected references are:

- `/opt/valheim/BepInEx/core/BepInEx.dll`
- `/opt/valheim/BepInEx/core/0Harmony.dll`
- `/opt/valheim/valheim_server_Data/Managed/UnityEngine.dll`
- `/opt/valheim/valheim_server_Data/Managed/UnityEngine.CoreModule.dll`
- `/opt/valheim/valheim_server_Data/Managed/assembly_valheim.dll`

Copy the resulting DLL into the Valheim server plugin folder when deploying:

```text
/home/bear/valheim_server/BepInEx/plugins/DiscordConnect/
```

## Configure

The default BepInEx configuration pattern is a `.cfg` file. Keep the secret in a local `.env` file and generate the server config from a template during deploy:

```text
/home/bear/valheim_server/BepInEx/config/
```

Local `.env` example:

```env
VALHEIM_DISCORD_WEBHOOK_URL=DISCORD_WEBHOOK_PLACEHOLDER
VALHEIM_DISCORD_POLL_INTERVAL_SECONDS=5
```

If `VALHEIM_DISCORD_WEBHOOK_URL` is present in `.env`, its value is copied into the generated config. Otherwise the deploy script inserts `DISCORD_WEBHOOK_PLACEHOLDER` so the config still renders correctly without a real secret. The default poll interval is `5` seconds.

The generated config template is `valheim.discordconnect.cfg.template`, which produces the server config file `valheim.discordconnect.cfg` at deploy time.

You can deploy the built DLL and generated config with the included `deploy.sh` script. It reads the `.env` values, builds the DLL, and uploads both files to:

- `/home/bear/valheim_server/BepInEx/plugins/DiscordConnect/`
- `/home/bear/valheim_server/BepInEx/config/`

The same files are also packaged locally under `install/`:

```text
install/plugin/BepInExDiscordConnect.dll
install/config/valheim.discordconnect.cfg
```

Run `bash ./package.sh` to rebuild the DLL and recreate this package. The packaged config contains a placeholder rather than the local webhook secret.

The plugin loads configuration in this order:

1. `.env`
2. `VALHEIM_DISCORD_WEBHOOK_URL` environment variable
3. `/home/bear/valheim_server/BepInEx/config/valheim.discordconnect.cfg` value for `WebhookUrl`

The plugin posts one startup message when the server loads it, then posts a message when a new player joins. Each join message includes the joining player and the current active-player list. No Discord bot process or gateway connection is required.

## Current POC scope

The plugin reports joins, sends Discord webhook notifications, and exposes the current player list through a localhost-only status bridge at `127.0.0.1:8765` for the separate `discord-bot` application. The webhook URL should be injected through the service manager environment where possible.