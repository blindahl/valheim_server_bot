#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$ROOT_DIR/.env"
LOCAL_VALHEIM_PATH="$ROOT_DIR/.valheim-references"
TEMPLATE_FILE="$ROOT_DIR/valheim.discordconnect.cfg.template"
OUT_FILE="$ROOT_DIR/valheim.discordconnect.cfg"
REMOTE_HOST="bear@192.168.1.171"
REMOTE_PLUGIN_DIR="/home/bear/valheim_server/BepInEx/plugins/DiscordConnect"
REMOTE_CONFIG_DIR="/home/bear/valheim_server/BepInEx/config"
REMOTE_TMP_DIR="/tmp/discordconnect_deploy_$$"

mkdir -p "$LOCAL_VALHEIM_PATH/BepInEx/core" "$LOCAL_VALHEIM_PATH/valheim_server_Data/Managed"
scp -o StrictHostKeyChecking=accept-new \
  "$REMOTE_HOST:/home/bear/valheim_server/BepInEx/core/BepInEx.dll" \
  "$REMOTE_HOST:/home/bear/valheim_server/BepInEx/core/0Harmony.dll" \
  "$LOCAL_VALHEIM_PATH/BepInEx/core/"
scp -o StrictHostKeyChecking=accept-new \
  "$REMOTE_HOST:/home/bear/valheim_server/valheim_server_Data/Managed/UnityEngine.dll" \
  "$REMOTE_HOST:/home/bear/valheim_server/valheim_server_Data/Managed/UnityEngine.CoreModule.dll" \
  "$REMOTE_HOST:/home/bear/valheim_server/valheim_server_Data/Managed/assembly_valheim.dll" \
  "$LOCAL_VALHEIM_PATH/valheim_server_Data/Managed/"

if [[ -f "$ENV_FILE" ]]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

WEBHOOK_URL="${VALHEIM_DISCORD_WEBHOOK_URL:-DISCORD_WEBHOOK_PLACEHOLDER}"

sed \
  -e "s|__VALHEIM_DISCORD_WEBHOOK_URL__|${WEBHOOK_URL//&/\\&}|g" \
  "$TEMPLATE_FILE" > "$OUT_FILE"

dotnet build -c Release -p:ValheimPath="$LOCAL_VALHEIM_PATH"

ssh -o StrictHostKeyChecking=accept-new "$REMOTE_HOST" "mkdir -p '$REMOTE_TMP_DIR' '$REMOTE_PLUGIN_DIR' '$REMOTE_CONFIG_DIR'"
scp -o StrictHostKeyChecking=accept-new "$ROOT_DIR/bin/Release/net472/BepInExDiscordConnect.dll" "$OUT_FILE" "$REMOTE_HOST:$REMOTE_TMP_DIR/"
ssh -o StrictHostKeyChecking=accept-new "$REMOTE_HOST" "cp '$REMOTE_TMP_DIR/BepInExDiscordConnect.dll' '$REMOTE_PLUGIN_DIR/' && cp '$REMOTE_TMP_DIR/$(basename "$OUT_FILE")' '$REMOTE_CONFIG_DIR/' && rm -rf '$REMOTE_TMP_DIR'"

echo "Deployed plugin and config to $REMOTE_HOST"
