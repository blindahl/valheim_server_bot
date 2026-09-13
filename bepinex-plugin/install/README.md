# BepInEx installation package

Copy the contents into the Valheim server installation:

```text
install/plugin/BepInExDiscordConnect.dll
  -> BepInEx/plugins/DiscordConnect/BepInExDiscordConnect.dll

install/config/valheim.discordconnect.cfg
  -> BepInEx/config/valheim.discordconnect.cfg
```

Before using the package, replace `DISCORD_WEBHOOK_PLACEHOLDER` in the config with the webhook URL, or use the repository `deploy.sh` workflow which generates the config from the local `.env` file.