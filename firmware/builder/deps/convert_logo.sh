#!/bin/bash
# Script to convert logo.png to 320x320 P3 PPM format.
# Overwrites logo_nest_clut224.ppm in the same directory.
# Requires ImageMagick (convert) and Netpbm (pnmtoplainpnm) to be installed.
# Cross-platform if these tools are available (install via package manager).

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PNG_PATH="$SCRIPT_DIR/logo.png"
PPM_PATH="$SCRIPT_DIR/logo_nest_clut224.ppm"
TEMP_PPM="$SCRIPT_DIR/temp.ppm"

# Check if logo.png exists
if [ ! -f "$PNG_PATH" ]; then
    echo "Error: $PNG_PATH not found."
    exit 1
fi

# Check for required tools
if ! command -v convert &> /dev/null; then
    echo "Error: ImageMagick (convert) not installed. Please install it."
    echo "  macOS: brew install imagemagick"
    echo "  Ubuntu/Debian: sudo apt install imagemagick"
    echo "  Other systems: install via package manager"
    exit 1
fi

if ! command -v pnmtoplainpnm &> /dev/null; then
    echo "Error: Netpbm (pnmtoplainpnm) not installed. Please install it."
    echo "  macOS: brew install netpbm"
    echo "  Ubuntu/Debian: sudo apt install netpbm"
    echo "  Other systems: install via package manager"
    exit 1
fi

# Resize to 320x320 and convert to PPM (P6 binary)
convert "$PNG_PATH" -resize 320x320! "$TEMP_PPM"

# Convert P6 PPM to P3 plain text PPM
pnmtoplainpnm "$TEMP_PPM" > "$PPM_PATH"

# Clean up temp file
rm "$TEMP_PPM"

echo "Successfully converted $PNG_PATH to $PPM_PATH"