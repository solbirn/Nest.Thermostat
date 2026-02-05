#!/usr/bin/env bash
set -euo pipefail

#
# Nest Thermostat - Build Firmware
#
# This script builds custom firmware for self-hosted deployments.
# It uses Docker to create a consistent build environment.
#

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILDER_DIR="$SCRIPT_DIR/builder"

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
  echo -e "${BOLD}${CYAN}║         ${BOLD}Nest - Build Firmware${CYAN}                      ║${NC}"
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

require_cmd() {
  local missing=()
  for cmd in "$@"; do
    if ! command -v "$cmd" >/dev/null 2>&1; then
      missing+=("$cmd")
    fi
  done

  if [ "${#missing[@]}" -ne 0 ]; then
    print_error "Missing required commands: ${missing[*]}"
    echo
    echo "Please install the missing dependencies:"
    for cmd in "${missing[@]}"; do
      case "$cmd" in
        docker)
          echo "  Docker: https://www.docker.com/products/docker-desktop"
          ;;
        *)
          echo "  $cmd: Install via your package manager"
          ;;
      esac
    done
    exit 1
  fi
}

prompt_value() {
  local prompt="$1"
  local default="${2:-}"
  local value

  if [ -n "$default" ]; then
    read -r -p "$prompt [$default]: " value
    value="${value:-$default}"
  else
    read -r -p "$prompt: " value
  fi

  value="${value//$'\n'/}"
  value="${value//$'\r'/}"

  echo "$value"
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

normalize_url() {
  local raw="$1"
  if [[ "$raw" != http://* && "$raw" != https://* ]]; then
    raw="https://$raw"
  fi
  raw="${raw%/}"
  echo "$raw"
}

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Build custom firmware for Nest thermostats (self-hosted).

Options:
  --api-url <url>        Your self-hosted server URL (required)
  --generation <gen>     Thermostat generation: gen1, gen2, or both (default: gen2)
  --enable-root-access   Enable SSH root access (password: nest)
  --custom-cert <path>   Embed a custom CA certificate instead of Mozilla's bundle
  --force-build          Force rebuild even if firmware exists
  --yes, -y              Non-interactive mode
  --help, -h             Show this help message

Examples:
  # Interactive build
  $(basename "$0")

  # Non-interactive build
  $(basename "$0") --api-url https://nest.example.com --generation gen2 --yes

  # Non-interactive build with custom CA certificate
  $(basename "$0") --api-url https://nest.example.com --custom-cert /path/to/ca.pem --yes

  # Build with SSH access enabled
  $(basename "$0") --api-url https://nest.example.com --enable-root-access --yes

Output:
  Firmware files are written to: builder/firmware/
    - uImage              Kernel image
    - u-boot.bin          U-Boot bootloader
    - x-load-gen1.bin     First-stage bootloader (Gen 1)
    - x-load-gen2.bin     First-stage bootloader (Gen 2)
EOF
}

# Parse arguments
API_URL=""
GENERATION="gen2"
ENABLE_ROOT_ACCESS=false
CUSTOM_CERT_PATH=""
FORCE_BUILD=false
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
      shift
      ;;
    --generation)
      shift
      if [ -z "${1:-}" ]; then
        print_error "--generation requires a value (gen1, gen2, or both)"
        exit 1
      fi
      GENERATION="$1"
      shift
      ;;
    --enable-root-access)
      ENABLE_ROOT_ACCESS=true
      shift
      ;;
    --custom-cert)
      shift
      if [ -z "${1:-}" ]; then
        print_error "--custom-cert requires a path to a PEM certificate file"
        exit 1
      fi
      CUSTOM_CERT_PATH="$1"
      shift
      ;;
    --force-build)
      FORCE_BUILD=true
      shift
      ;;
    --yes|-y)
      NON_INTERACTIVE=true
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

print_section "Checking Prerequisites"

require_cmd docker
print_success "Docker found"

# Check Docker is running
if ! docker info >/dev/null 2>&1; then
  print_error "Docker is not running"
  echo
  echo "Please start Docker Desktop and try again."
  exit 1
fi
print_success "Docker is running"

# Interactive prompts if needed
if [ "$NON_INTERACTIVE" = false ]; then
  print_section "Configuration"
  
  echo -e "${BOLD}Self-Hosted Server Configuration${NC}"
  echo
  echo "Enter the URL of your self-hosted server. This is where your Nest"
  echo "thermostat will connect after flashing the custom firmware."
  echo
  echo "Example: https://nest.example.com"
  echo
  
  if [ -z "$API_URL" ]; then
    API_URL=$(prompt_value "API URL" "")
    
    if [ -z "$API_URL" ]; then
      print_error "API URL is required for self-hosted setup"
      exit 1
    fi
  fi
  
  API_URL=$(normalize_url "$API_URL")
  
  echo
  echo -e "${BOLD}Thermostat Generation${NC}"
  echo
  echo "  Gen 1: Original Diamond Nest thermostat (older)"
  echo "  Gen 2: j49 Nest thermostat (more common)"
  echo
  
  if prompt_yes_no "Are you building for Gen 2?" "y"; then
    GENERATION="gen2"
  else
    GENERATION="gen1"
  fi
  
  echo
  if prompt_yes_no "Enable SSH root access? (for debugging)" "n"; then
    ENABLE_ROOT_ACCESS=true
    print_warning "Root access will be enabled (password: nest)"
  fi
fi

# Validate API URL
if [ -z "$API_URL" ]; then
  print_error "API URL is required. Use --api-url or run in interactive mode."
  echo
  usage
  exit 1
fi

API_URL=$(normalize_url "$API_URL")

# Build firmware
print_section "Building Firmware"

echo -e "${BOLD}Build Configuration:${NC}"
echo "  API URL:         $API_URL"
echo "  Generation:      $GENERATION"
if [ -n "$CUSTOM_CERT_PATH" ]; then
  echo "  SSL CA:          Custom certificate ($CUSTOM_CERT_PATH)"
else
  echo "  SSL CA:          Mozilla CA bundle (public SSL)"
fi
echo "  Root Access:     $([ "$ENABLE_ROOT_ACCESS" = true ] && echo "Enabled" || echo "Disabled")"
echo

if [ "$NON_INTERACTIVE" = false ]; then
  if ! prompt_yes_no "Proceed with firmware build?" "y"; then
    print_warning "Build cancelled"
    exit 0
  fi
fi

cd "$BUILDER_DIR"

BUILD_ARGS=(
  --api-url "$API_URL"
  --generation "$GENERATION"
  --yes
)

if [ -n "$CUSTOM_CERT_PATH" ]; then
  # Resolve to absolute path for Docker mount
  CUSTOM_CERT_PATH="$(cd "$(dirname "$CUSTOM_CERT_PATH")" && pwd)/$(basename "$CUSTOM_CERT_PATH")"
  if [ ! -f "$CUSTOM_CERT_PATH" ]; then
    print_error "Certificate file not found: $CUSTOM_CERT_PATH"
    exit 1
  fi
  BUILD_ARGS+=(--custom-cert /custom-cert/ca.pem)
else
  BUILD_ARGS+=(--public-ssl)
fi

if [ "$ENABLE_ROOT_ACCESS" = true ]; then
  BUILD_ARGS+=(--enable-root-access)
fi

if [ "$FORCE_BUILD" = true ]; then
  BUILD_ARGS+=(--force-build)
fi

print_info "Starting Docker build..."
echo

# Export custom cert path for docker-build.sh to mount
export NEST_CUSTOM_CERT_PATH="${CUSTOM_CERT_PATH:-}"

if ./docker-build.sh "${BUILD_ARGS[@]}"; then
  echo
  print_success "Firmware build complete!"
  echo
  print_section "Build Output"
  echo "Firmware files are in: $BUILDER_DIR/firmware/"
  echo
  ls -lh "$BUILDER_DIR/firmware/" 2>/dev/null || echo "  (no files found)"
  echo
  echo "Next step: Flash firmware to your device with:"
  echo
  echo "  ./flash-firmware.sh --generation $GENERATION"
  echo
else
  print_error "Firmware build failed"
  exit 1
fi
