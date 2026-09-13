#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_DIR="$ROOT_DIR/install"

mkdir -p "$PACKAGE_DIR"
rm -rf "$PACKAGE_DIR/app" "$PACKAGE_DIR/systemd" "$PACKAGE_DIR/config" \
	"$PACKAGE_DIR/PRIVACY_POLICY.md" "$PACKAGE_DIR/TERMS_OF_SERVICE.md"
mkdir -p "$PACKAGE_DIR/app" "$PACKAGE_DIR/systemd" "$PACKAGE_DIR/config"

cp "$ROOT_DIR/discord_bot.py" "$PACKAGE_DIR/app/"
cp "$ROOT_DIR/requirements.txt" "$PACKAGE_DIR/app/"
cp "$ROOT_DIR/discordconnect-bot.service" "$PACKAGE_DIR/systemd/"
cp "$ROOT_DIR/.env.example" "$PACKAGE_DIR/config/"
cp "$ROOT_DIR/PRIVACY_POLICY.md" "$PACKAGE_DIR/PRIVACY_POLICY.md"
cp "$ROOT_DIR/TERMS_OF_SERVICE.md" "$PACKAGE_DIR/TERMS_OF_SERVICE.md"

printf 'Discord bot package created in %s\n' "$PACKAGE_DIR"
