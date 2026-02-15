#!/bin/bash
set -euo pipefail

# Spyder Tally Controller - Install Script
# This script installs the Spyder Tally Controller application on a Raspberry Pi.
# It auto-detects architecture (arm64 vs arm32) and installs the correct binary.
#
# Usage:
#   tar xzf spyder-tally-linux-arm64.tar.gz   # (or linux-arm)
#   cd spyder-tally-linux-arm64/               # (or linux-arm)
#   sudo ./install.sh

INSTALL_DIR="/opt/spyder-tally"
SERVICE_NAME="SpyderTallies"

# --- Preflight checks ---

if [ "$(id -u)" -ne 0 ]; then
    echo "Error: This script must be run as root (use sudo ./install.sh)"
    exit 1
fi

# Determine the real user who invoked sudo
INSTALL_USER="${SUDO_USER:-$(whoami)}"
if [ "$INSTALL_USER" = "root" ]; then
    echo "Error: Please run this script with sudo from a regular user account, not as root directly."
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# --- Architecture detection ---

ARCH="$(uname -m)"
case "$ARCH" in
    aarch64)
        EXPECTED_RID="linux-arm64"
        ;;
    armv7l|armv6l)
        EXPECTED_RID="linux-arm"
        ;;
    *)
        echo "Error: Unsupported architecture '$ARCH'. Expected aarch64 (arm64) or armv7l/armv6l (arm32)."
        exit 1
        ;;
esac

# Check that the binary in this package matches the detected architecture
if [ ! -f "$SCRIPT_DIR/bin/SpyderTallyControllerWebApp" ]; then
    echo "Error: Binary not found at $SCRIPT_DIR/bin/SpyderTallyControllerWebApp"
    echo ""
    echo "Detected architecture: $ARCH ($EXPECTED_RID)"
    echo "Make sure you downloaded the correct package for your Pi:"
    echo "  - Raspberry Pi 4/5 (64-bit OS): spyder-tally-linux-arm64.tar.gz"
    echo "  - Raspberry Pi 3 or older (32-bit OS): spyder-tally-linux-arm.tar.gz"
    exit 1
fi

echo "=== Spyder Tally Controller Installer ==="
echo "Detected architecture: $ARCH ($EXPECTED_RID)"
echo "Installing as user: $INSTALL_USER"
echo "Install directory: $INSTALL_DIR"
echo ""

# --- Stop existing service if running ---

if systemctl is-active --quiet "$SERVICE_NAME" 2>/dev/null; then
    echo "Stopping existing $SERVICE_NAME service..."
    systemctl stop "$SERVICE_NAME"
fi

# --- Enable I2C ---

echo "Enabling I2C interface..."
if command -v raspi-config &> /dev/null; then
    raspi-config nonint do_i2c 0
else
    echo "  Warning: raspi-config not found - skipping I2C setup."
    echo "  You may need to enable I2C manually if not already enabled."
fi

# --- Install application ---

echo "Installing application to $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"

# Copy binary
cp "$SCRIPT_DIR/bin/SpyderTallyControllerWebApp" "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/SpyderTallyControllerWebApp"

# Copy static web assets (CSS, JS, images, etc.)
if [ -d "$SCRIPT_DIR/bin/wwwroot" ]; then
    cp -r "$SCRIPT_DIR/bin/wwwroot" "$INSTALL_DIR/"
    echo "  Installed static web assets"
fi

# Copy default config files (only if they don't already exist, to preserve user settings on upgrade)
for config_file in appConfig.json deviceConfig.json; do
    if [ ! -f "$INSTALL_DIR/$config_file" ]; then
        cp "$SCRIPT_DIR/$config_file" "$INSTALL_DIR/"
        echo "  Created default $config_file"
    else
        echo "  Keeping existing $config_file (not overwriting)"
    fi
done

# Set ownership so the service user can write config files
chown -R "$INSTALL_USER":"$INSTALL_USER" "$INSTALL_DIR"

# --- Install systemd service ---

echo "Installing systemd service..."
sed "s/__USER__/$INSTALL_USER/g" "$SCRIPT_DIR/SpyderTallies.service" > /etc/systemd/system/SpyderTallies.service
systemctl daemon-reload
systemctl enable "$SERVICE_NAME"
systemctl start "$SERVICE_NAME"

# --- Done ---

echo ""
echo "=== Installation complete! ==="
echo ""
echo "The Spyder Tally Controller is now running."
echo "  Web UI: http://$(hostname -I | awk '{print $1}')"
echo ""
echo "Useful commands:"
echo "  sudo systemctl status $SERVICE_NAME    # Check service status"
echo "  sudo systemctl restart $SERVICE_NAME   # Restart the service"
echo "  sudo journalctl -u $SERVICE_NAME -f    # Tail the log"
