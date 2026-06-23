#!/usr/bin/env bash
set -euo pipefail

APP_NAME="RimeDictManager"
APP_ID="rimedictmanager"
INSTALL_DIR="$HOME/.local/share/$APP_NAME"
ICON_DIR="$HOME/.local/share/icons/hicolor"
DESKTOP_DIR="$HOME/.local/share/applications"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "安装 $APP_NAME..."

echo "  安装应用文件..."
mkdir -p "$INSTALL_DIR"
rm -rf "${INSTALL_DIR:?}/"*
cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/"
rm -f "$INSTALL_DIR/install.sh"

echo "  安装图标..."
mkdir -p "$ICON_DIR/256x256/apps"
cp "$SCRIPT_DIR/icon_256.png" "$ICON_DIR/256x256/apps/$APP_ID.png"
mkdir -p "$ICON_DIR/48x48/apps"
cp "$SCRIPT_DIR/icon_48.png" "$ICON_DIR/48x48/apps/$APP_ID.png"

echo "  安装桌面条目..."
mkdir -p "$DESKTOP_DIR"
cp "$SCRIPT_DIR/$APP_ID.desktop" "$DESKTOP_DIR/"
sed -i "s|Exec=$APP_NAME|Exec=$INSTALL_DIR/$APP_NAME|" "$DESKTOP_DIR/$APP_ID.desktop"

echo "  更新桌面数据库..."
if command -v update-desktop-database &> /dev/null; then
    update-desktop-database "$DESKTOP_DIR"
fi
if command -v gtk-update-icon-cache &> /dev/null; then
    gtk-update-icon-cache -q -t -f "$ICON_DIR" 2>/dev/null || true
fi

echo "应用已安装到: $INSTALL_DIR"
echo "桌面条目: $DESKTOP_DIR/$APP_ID.desktop"
echo "如需卸载，删除以下目录/文件即可："
echo "  $INSTALL_DIR"
echo "  $DESKTOP_DIR/$APP_ID.desktop"
echo "  $ICON_DIR/256x256/apps/$APP_ID.png"
echo "  $ICON_DIR/48x48/apps/$APP_ID.png"
