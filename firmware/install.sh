#!/usr/bin/env bash
set -euo pipefail

#
# Nest Thermostat - Combined Build & Flash
#
# This script combines firmware building and flashing into one workflow.
# For individual steps, use build-firmware.sh or flash-firmware.sh directly.
#

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Colors for output
if [ -t 1 ]; then
  RED='\033[0;31m'
  GREEN='\033[0;32m'
  YELLOW='\033[1;33m'
  BLUE='\033[0;34m'
  CYAN='\033[0;36m'
  BOLD='\033[1m'
  NC='\033[0m'
else
  RED=''
  GREEN=''
  YELLOW=''
  BLUE=''
  CYAN=''
  BOLD=''
  NC=''
fi

print_header() {
  echo
  echo -e "${BOLD}${CYAN}╔═══════════════════════════════════════════════════════════════╗${NC}"
  echo -e "${BOLD}${CYAN}║                                                               ║${NC}"
  echo -e "${BOLD}${CYAN}║         ${BOLD}Nest Firmware Setup${CYAN}                        ║${NC}"
  echo -e "${BOLD}${CYAN}║                   Self-Hosted Edition                         ║${NC}"
  echo -e "${BOLD}${CYAN}║                                                               ║${NC}"
  echo -e "${BOLD}${CYAN}╚═══════════════════════════════════════════════════════════════╝${NC}"
  echo
}

print_success() { echo -e "${GREEN}[✓]${NC} $1"; }
print_info() { echo -e "${CYAN}[→]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[!]${NC} $1"; }
print_error() { echo -e "${RED}[✗]${NC} $1"; }

print_section() {
  echo
  echo -e "${BOLD}${BLUE}┌───────────────────────────────────────────────────────────────┐${NC}"
  echo -e "${BOLD}${BLUE}│${NC} $1"
  echo -e "${BOLD}${BLUE}└───────────────────────────────────────────────────────────────┘${NC}"
  echo
}

prompt_yes_no() {
  local prompt="$1"
  local default="$2"
  local options response

  if [ "$default" = "y" ]; then
    options="[Y/n]"
  else
    options="[y/N]"
  fi

  while true; do
    read -r -p "$prompt $options " response
    response="${response:-$default}"
    case "$response" in
      [Yy]*) return 0 ;;
      [Nn]*) return 1 ;;
      *) echo "Please answer y or n." ;;
    esac
  done
}

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Combined firmware build and flash for Nest thermostats (self-hosted).

This script runs both build-firmware.sh and flash-firmware.sh in sequence.
For individual steps, use those scripts directly.

Options:
  --api-url <url>        Your self-hosted server URL (required)
  --generation <gen>     Thermostat generation: gen1, gen2, or both (default: gen2)
  --enable-root-access   Enable SSH root access (password: nest)
  --force-build          Force rebuild even if firmware exists
  --skip-flash           Build only, skip flashing
  --yes, -y              Non-interactive mode
  --help, -h             Show this help message

Individual Scripts:
  ./build-firmware.sh    Build firmware only
  ./flash-firmware.sh    Flash firmware only (also builds omap_loader if needed)

Examples:
  # Interactive build and flash
  $(basename "$0")

  # Non-interactive build and flash
  $(basename "$0") --api-url https://nest.example.com --generation gen2 --yes

  # Build only (same as ./build-firmware.sh)
  $(basename "$0") --api-url https://nest.example.com --skip-flash --yes
EOF
}

# Parse arguments - collect them to pass to sub-scripts
BUILD_ARGS=()
FLASH_ARGS=()
API_URL=""
GENERATION="gen2"
SKIP_FLASH=false
NON_INTERACTIVE=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --api-url)
      shift
      if [ -z "${1:-}" ]; then
        print_error "--api-url requires a value"
        exit 1
      fi
      API_URL="$1"
      BUILD_ARGS+=(--api-url "$1")
      shift
      ;;
    --generation)
      shift
      if [ -z "${1:-}" ]; then
        print_error "--generation requires a value (gen1, gen2, or both)"
        exit 1
      fi
      GENERATION="$1"
      BUILD_ARGS+=(--generation "$1")
      FLASH_ARGS+=(--generation "$1")
      shift
      ;;
    --enable-root-access)
      BUILD_ARGS+=(--enable-root-access)
      shift
      ;;
    --force-build)
      BUILD_ARGS+=(--force-build)
      shift
      ;;
    --skip-flash)
      SKIP_FLASH=true
      shift
      ;;
    --yes|-y)
      NON_INTERACTIVE=true
      BUILD_ARGS+=(--yes)
      shift
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      print_error "Unknown option: $1"
      usage
      exit 1
      ;;
  esac
done

# Main script
print_header

print_section "Workflow Overview"

echo "This script will:"
echo "  1. Build custom firmware with your server URL embedded"
echo "  2. Build omap_loader for your platform (if needed)"
echo "  3. Flash the firmware to your Nest thermostat"
echo
echo "Individual steps can be run separately:"
echo "  ./build-firmware.sh    Build firmware only"
echo "  ./flash-firmware.sh    Flash firmware (builds omap_loader if needed)"
echo

# Step 1: Build firmware
print_section "Step 1: Build Firmware"

if ! "$SCRIPT_DIR/build-firmware.sh" "${BUILD_ARGS[@]}"; then
  print_error "Firmware build failed"
  exit 1
fi

# Step 2: Flash (if not skipped)
if [ "$SKIP_FLASH" = true ]; then
  echo
  print_success "Build complete! Skipping flash as requested."
  echo
  echo "To flash later, run:"
  echo "  ./flash-firmware.sh --generation $GENERATION"
  echo
else
  print_section "Step 2: Flash Firmware"
  
  if [ "$NON_INTERACTIVE" = false ]; then
    echo
    if ! prompt_yes_no "Ready to flash firmware to device?" "y"; then
      print_warning "Flash skipped"
      echo
      echo "To flash later, run:"
      echo "  ./flash-firmware.sh --generation $GENERATION"
      echo
      exit 0
    fi
  fi
  
  if ! "$SCRIPT_DIR/flash-firmware.sh" "${FLASH_ARGS[@]}"; then
    print_error "Flash failed"
    exit 1
  fi
fi

echo
print_success "All done!"
