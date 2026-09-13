# Discord bot installation package

The package contains the standalone Discord bot and its systemd service files.

## Contents

```text
install/
├── app/
│   ├── discord_bot.py
│   └── requirements.txt
├── config/
│   └── .env.example
├── systemd/
│   └── discordconnect-bot.service
├── PRIVACY_POLICY.md
├── TERMS_OF_SERVICE.md
└── README.md
```

## Install on the Valheim server

Copy this `install` folder to the server, change into it, and run:

```bash
cd /path/to/install
sudo mkdir -p /home/bear/discord-bot
sudo cp app/* /home/bear/discord-bot/
sudo cp config/.env.example /home/bear/discord-bot/.env
sudo cp systemd/discordconnect-bot.service /etc/systemd/system/
sudo chown -R bear:bear /home/bear/discord-bot
cd /home/bear/discord-bot
sudo apt install python3.12-venv
python3 -m venv .venv
.venv/bin/pip install -r requirements.txt
nano .env
chmod 600 .env
sudo systemctl daemon-reload
sudo systemctl enable --now discordconnect-bot.service
```

Set `DISCORD_BOT_TOKEN` and `DISCORD_COMMAND_CHANNEL_ID` in `.env` before starting the service. Never commit `.env` or share the token.

The bot expects the BepInEx plugin status bridge at `http://127.0.0.1:8765/players` and reads the allowed Discord user IDs from:

```text
/home/bear/valheim_server/BepInEx/config/discordconnect-allowed-users.txt
```
