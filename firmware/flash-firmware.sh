#!/usr/bin/env bash
set -euo pipefail

#
# Nest Thermostat - Flash Firmware
#
# This script flashes firmware to a Nest thermostat via USB DFU mode.
# It will build omap_loader for your platform if not already built.
#

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIRMWARE_DIR="$SCRIPT_DIR/builder/firmware"
OMAP_DIR="$SCRIPT_DIR/omap_loader"
BIN_DIR="$OMAP_DIR/bin"
SRC_DIR="$OMAP_DIR/src"
PATCH_DIR="$OMAP_DIR/patches"

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
  echo -e "${BOLD}${CYAN}║         ${BOLD}Nest - Flash Firmware${CYAN}                      ║${NC}"
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

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Flash firmware to a Nest thermostat via USB DFU mode.

Options:
  --generation <gen>     Thermostat generation: gen1 or gen2 (default: gen2)
  --build-only           Only build omap_loader, don't flash
  --help, -h             Show this help message

Requirements:
  - Device must be in DFU mode (see instructions below)
  - libusb must be installed (brew install libusb on macOS)
  - sudo access for USB device communication

DFU Mode Instructions:
  1. Remove thermostat from wall mount (pull straight out)
  2. Hold the ring (don't press, just hold down)
  3. While holding, connect USB cable to computer
  4. Keep holding for 10 seconds until screen goes blank
  5. Release ring - device is now in DFU mode

Examples:
  # Flash Gen 2 thermostat
  $(basename "$0")

  # Flash Gen 1 thermostat  
  $(basename "$0") --generation gen1

  # Just build omap_loader without flashing
  $(basename "$0") --build-only
EOF
}

# Detect platform and get target directory
get_platform_info() {
  local os arch platform target_dir binary_name
  
  os="$(uname -s)"
  arch="$(uname -m)"
  
  case "$os" in
    Darwin)
      if [ "$arch" = "arm64" ]; then
        platform="macos-arm64"
      else
        platform="macos-x64"
      fi
      binary_name="omap_loader"
      ;;
    Linux)
      platform="linux-x64"
      binary_name="omap_loader"
      ;;
    MINGW*|MSYS*|CYGWIN*)
      platform="windows-x64"
      binary_name="omap_loader.exe"
      ;;
    *)
      print_error "Unsupported operating system: $os"
      exit 1
      ;;
  esac
  
  target_dir="$BIN_DIR/$platform"
  
  echo "$platform|$target_dir|$binary_name"
}

# Build omap_loader for current platform
build_omap_loader() {
  local platform target_dir binary_name
  local platform_info
  
  platform_info=$(get_platform_info)
  platform=$(echo "$platform_info" | cut -d'|' -f1)
  target_dir=$(echo "$platform_info" | cut -d'|' -f2)
  binary_name=$(echo "$platform_info" | cut -d'|' -f3)
  
  print_section "Building omap_loader for $platform"
  
  local os="$(uname -s)"
  
  # Check for required dependencies
  case "$os" in
    Darwin)
      if ! command -v gcc &> /dev/null && ! command -v clang &> /dev/null; then
        print_error "Compiler not found. Install Xcode Command Line Tools:"
        echo "  xcode-select --install"
        exit 1
      fi
      print_success "Compiler found"
      
      # Check for libusb
      if ! brew list libusb &> /dev/null 2>&1 && ! pkg-config --exists libusb-1.0 2>/dev/null; then
        print_warning "libusb not found. Attempting to install..."
        
        if command -v brew &> /dev/null; then
          print_info "Installing libusb via Homebrew..."
          brew install libusb
        else
          print_error "Homebrew not found. Please install libusb manually:"
          echo "  1. Install Homebrew: https://brew.sh"
          echo "  2. Run: brew install libusb"
          exit 1
        fi
      fi
      print_success "libusb found"
      ;;
      
    Linux)
      if ! command -v gcc &> /dev/null; then
        print_error "GCC not found. Install with:"
        echo "  sudo apt-get install build-essential"
        exit 1
      fi
      print_success "GCC found"
      
      if ! pkg-config --exists libusb-1.0 2>/dev/null; then
        print_error "libusb-1.0 not found. Install with:"
        echo "  sudo apt-get install libusb-1.0-0-dev"
        exit 1
      fi
      print_success "libusb found"
      ;;
  esac
  
  # Apply macOS patch if needed
  if [ "$os" = "Darwin" ]; then
    local patch_file="$PATCH_DIR/omap_loader_mac.patch"
    if [ -f "$patch_file" ]; then
      print_info "Checking macOS patch..."
      cd "$SRC_DIR"
      if patch -p0 --dry-run --silent < "$patch_file" 2>&1 | grep -q "previously applied"; then
        print_success "macOS patch already applied"
      elif patch -p0 --dry-run --silent < "$patch_file" > /dev/null 2>&1; then
        print_info "Applying macOS patch..."
        patch -p0 < "$patch_file"
        print_success "macOS patch applied"
      else
        print_warning "Patch may already be applied or not needed"
      fi
    fi
  fi
  
  # Build
  print_info "Compiling omap_loader..."
  cd "$SRC_DIR"
  make clean 2>/dev/null || true
  make
  
  # Install to bin directory
  mkdir -p "$target_dir"
  cp "$binary_name" "$target_dir/"
  chmod +x "$target_dir/$binary_name"
  
  print_success "Built: $target_dir/$binary_name"
  
  # Copy libusb dylib on macOS for portability
  if [ "$os" = "Darwin" ]; then
    local libusb_path
    libusb_path=$(brew --prefix libusb 2>/dev/null)/lib/libusb-1.0.0.dylib || true
    if [ -f "$libusb_path" ]; then
      cp "$libusb_path" "$target_dir/"
      print_success "Copied libusb to $target_dir/"
    fi
  fi
  
  echo
}

# Get path to omap_loader binary, building if necessary
get_omap_loader() {
  local platform_info platform target_dir binary_name loader_path
  
  platform_info=$(get_platform_info)
  platform=$(echo "$platform_info" | cut -d'|' -f1)
  target_dir=$(echo "$platform_info" | cut -d'|' -f2)
  binary_name=$(echo "$platform_info" | cut -d'|' -f3)
  
  loader_path="$target_dir/$binary_name"
  
  # Check if binary exists
  if [ ! -f "$loader_path" ]; then
    print_warning "omap_loader not found for $platform"
    echo
    build_omap_loader
  fi
  
  # Verify it exists now
  if [ ! -f "$loader_path" ]; then
    print_error "Failed to build omap_loader"
    exit 1
  fi
  
  # Ensure executable
  chmod +x "$loader_path"
  
  echo "$loader_path"
}

flash_firmware() {
  local gen="$1"
  local loader_path
  
  # Get omap_loader (builds if necessary)
  loader_path=$(get_omap_loader)
  
  print_section "Preparing to Flash"
  
  # Determine firmware files
  local xload_file="$FIRMWARE_DIR/x-load-${gen}.bin"
  local uboot_file="$FIRMWARE_DIR/u-boot.bin"
  local uimage_file="$FIRMWARE_DIR/uImage"
  
  # Check firmware files exist
  local missing=()
  [ ! -f "$xload_file" ] && missing+=("x-load-${gen}.bin")
  [ ! -f "$uboot_file" ] && missing+=("u-boot.bin")
  [ ! -f "$uimage_file" ] && missing+=("uImage")
  
  if [ "${#missing[@]}" -ne 0 ]; then
    print_error "Missing firmware files: ${missing[*]}"
    echo
    echo "Please build firmware first with:"
    echo "  ./build-firmware.sh --api-url <your-url> --generation $gen"
    exit 1
  fi
  
  print_success "Firmware files found:"
  echo "  x-load:  $(basename "$xload_file") ($(du -h "$xload_file" | cut -f1))"
  echo "  u-boot:  $(basename "$uboot_file") ($(du -h "$uboot_file" | cut -f1))"
  echo "  uImage:  $(basename "$uimage_file") ($(du -h "$uimage_file" | cut -f1))"
  echo
  
  print_info "Using omap_loader: $loader_path"
  echo
  
  print_section "DFU Mode Instructions"
  
  echo -e "${BOLD}${YELLOW}════════════════════════════════════════════════════════════${NC}"
  echo -e "${BOLD}${YELLOW}                   PUT NEST IN DFU MODE                      ${NC}"
  echo -e "${BOLD}${YELLOW}════════════════════════════════════════════════════════════${NC}"
  echo
  echo -e "${BOLD}To put your Nest in DFU mode:${NC}"
  echo "  1. Remove thermostat from wall mount (pull straight out)"
  echo "  2. Hold the ring (don't press, just hold down)"
  echo "  3. While holding, connect USB cable to computer"
  echo "  4. Keep holding for 10 seconds until screen goes blank"
  echo "  5. Release ring - device is now in DFU mode"
  echo
  echo "The display will be black/blank. This is expected."
  echo
  
  read -r -p "Press ENTER when device is in DFU mode and connected..."
  echo
  
  print_info "Flashing firmware (requires sudo for USB access)..."
  echo
  
  # Show the command we're running
  echo -e "${BOLD}Running:${NC}"
  echo "  sudo $loader_path \\"
  echo "    -f $xload_file \\"
  echo "    -f $uboot_file -a 0x80100000 \\"
  echo "    -f $uimage_file -a 0x80A00000 \\"
  echo "    -j 0x80100000"
  echo
  
  # Execute the flash command
  sudo "$loader_path" \
    -f "$xload_file" \
    -f "$uboot_file" -a 0x80100000 \
    -f "$uimage_file" -a 0x80A00000 \
    -j 0x80100000
  
  local exit_code=$?
  
  if [ $exit_code -eq 0 ]; then
    echo
    print_success "Firmware flashed successfully!"
    echo
    echo -e "${BOLD}${GREEN}════════════════════════════════════════════════════════════${NC}"
    echo -e "${BOLD}${GREEN}                   FLASH COMPLETE!                          ${NC}"
    echo -e "${BOLD}${GREEN}════════════════════════════════════════════════════════════${NC}"
    echo
    echo "Your Nest thermostat will now reboot with the custom firmware."
    echo "It will connect to your self-hosted server once on WiFi."
    echo
  else
    print_error "Flash failed with exit code: $exit_code"
    echo
    echo "Troubleshooting:"
    echo "  - Is the device in DFU mode? (screen should be blank)"
    echo "  - Is the USB cable connected properly?"
    echo "  - Try a different USB port or cable"
    echo "  - Check dmesg/system.log for USB errors"
    exit $exit_code
  fi
}

# Parse arguments
GENERATION="gen2"
BUILD_ONLY=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --generation)
      shift
      if [ -z "${1:-}" ]; then
        print_error "--generation requires a value (gen1 or gen2)"
        exit 1
      fi
      GENERATION="$1"
      shift
      ;;
    --build-only)
      BUILD_ONLY=true
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

# Validate generation
case "$GENERATION" in
  gen1|gen2) ;;
  *)
    print_error "Invalid generation: $GENERATION (must be gen1 or gen2)"
    exit 1
    ;;
esac

# Main script
print_header

if [ "$BUILD_ONLY" = true ]; then
  build_omap_loader
  echo
  print_success "omap_loader built successfully!"
  echo
  echo "To flash firmware, run:"
  echo "  ./flash-firmware.sh --generation $GENERATION"
  echo
else
  flash_firmware "$GENERATION"
fi
