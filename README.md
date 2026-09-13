# Valheim Discord Bot

A private Discord bot that listens for `!players`, checks a server-side Discord user whitelist, and queries the Valheim BepInEx plugin through its localhost-only status bridge.

The bot is a separate application from the BepInEx plugin. The plugin runs inside Valheim; this bot runs independently as a systemd service.

## Local setup

```bash
python3 -m venv .venv
.venv/bin/pip install -r requirements.txt
cp .env.example .env
```

Set the bot token and command channel ID in `.env`. Never commit `.env` or share the token.

## Server setup

For a ready-to-copy installation package, run:

```bash
bash ./package.sh
```

This creates an `install/` folder containing `app/`, `config/`, and `systemd/` directories. The package contains only `.env.example`; it never contains the real bot token.

The bot expects the Valheim plugin to expose:

```text
http://127.0.0.1:8765/players
```

Install it on the Valheim server as `/home/bear/discord-bot`, create `.venv`, install `requirements.txt`, and configure `.env`. The whitelist is:

```text
/home/bear/valheim_server/BepInEx/config/discordconnect-allowed-users.txt
```

Inline comments are supported:

```text
123456789012345678 # Bjorn
```

Install the service:

```bash
sudo cp discordconnect-bot.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now discordconnect-bot.service
sudo journalctl -u discordconnect-bot.service -f
```

## Discord setup

Use the existing Discord application. Enable **Message Content Intent**, install the bot into the private Discord server with permission to view channels, send messages, and read message history, then use:

```text
!players
```

Only users in the whitelist and messages in the configured command channel are accepted.
