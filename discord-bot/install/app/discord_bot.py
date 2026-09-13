import asyncio
import json
import os
from pathlib import Path
from urllib.error import URLError
from urllib.request import urlopen

import discord

COMMAND_PREFIX = os.getenv("DISCORD_COMMAND_PREFIX", "!")
COMMAND_CHANNEL_ID = int(os.getenv("DISCORD_COMMAND_CHANNEL_ID", "0"))
STATUS_URL = os.getenv("VALHEIM_STATUS_URL", "http://127.0.0.1:8765/players")
WHITELIST_FILE = Path(os.getenv(
    "VALHEIM_DISCORD_ALLOWED_USERS_FILE",
    "/home/bear/valheim_server/BepInEx/config/discordconnect-allowed-users.txt",
))


def load_allowed_user_ids():
    if not WHITELIST_FILE.exists():
        return set()

    allowed_ids = set()
    for line in WHITELIST_FILE.read_text(encoding="utf-8").splitlines():
        value = line.split("#", 1)[0].strip()
        if value.isdigit():
            allowed_ids.add(int(value))
    return allowed_ids


def get_players():
    with urlopen(STATUS_URL, timeout=3) as response:
        payload = json.loads(response.read().decode("utf-8"))
    return payload.get("players", [])


def format_players(players):
    if not players:
        return "No players are currently online."
    return "Active players ({}):\n{}".format(len(players), "\n".join(players))


class ValheimBot(discord.Client):
    async def on_ready(self):
        guilds = ", ".join(f"{guild.name} ({guild.id})" for guild in self.guilds) or "none"
        print(f"Logged in as {self.user}. Joined servers: {guilds}", flush=True)

    async def on_message(self, message):
        if message.author.bot or not message.content.startswith(f"{COMMAND_PREFIX}players"):
            return
        if COMMAND_CHANNEL_ID and message.channel.id != COMMAND_CHANNEL_ID:
            return
        if message.author.id not in load_allowed_user_ids():
            await message.reply("You are not authorized to use Valheim commands.", mention_author=False)
            return

        command = message.content.strip()
        if command != f"{COMMAND_PREFIX}players":
            return

        try:
            players = await asyncio.to_thread(get_players)
            await message.reply(format_players(players), mention_author=False)
        except (OSError, URLError, ValueError, json.JSONDecodeError) as error:
            print(f"Could not query Valheim status: {error}")
            await message.reply("The Valheim status bridge is currently unavailable.", mention_author=False)


intents = discord.Intents.default()
intents.message_content = True
bot = ValheimBot(intents=intents)
bot.run(os.environ["DISCORD_BOT_TOKEN"])
