#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_DIR="$ROOT_DIR/install"

dotnet build -c Release -p:ValheimPath="$ROOT_DIR/.valheim-references"

mkdir -p "$PACKAGE_DIR/plugin" "$PACKAGE_DIR/config"
rm -f "$PACKAGE_DIR/config"/*.cfg
cp "$ROOT_DIR/bin/Release/net472/BepInExDiscordConnect.dll" "$PACKAGE_DIR/plugin/"
sed \
  -e 's|__VALHEIM_DISCORD_WEBHOOK_URL__|DISCORD_WEBHOOK_PLACEHOLDER|g' \
  "$ROOT_DIR/valheim.discordconnect.cfg.template" \
  > "$PACKAGE_DIR/config/valheim.discordconnect.cfg"

echo "Package created in $PACKAGE_DIR"