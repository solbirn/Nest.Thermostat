# Nest Thermostat - Standalone Firmware Package

Self-contained firmware build and flash tools for Nest thermostats with self-hosted server support.

## Quick Start

```bash
# Interactive build and flash
./install.sh

# Or step by step:
./build-firmware.sh --api-url https://nest.example.com
./flash-firmware.sh
```

## Prerequisites

### For Building Firmware
- **Docker** - [Install Docker Desktop](https://www.docker.com/products/docker-desktop)

### For Flashing Firmware
- **libusb** - USB communication library
- **Compiler** - For building omap_loader (auto-detected)

```bash
# macOS
brew install libusb

# Linux
sudo apt-get install libusb-1.0-0-dev build-essential
```

## Scripts

| Script | Purpose |
|--------|---------|
| `install.sh` | Combined workflow: build + flash |
| `build-firmware.sh` | Build firmware only (requires Docker) |
| `flash-firmware.sh` | Flash firmware only (builds omap_loader if needed) |

## Usage

### Option 1: Combined Workflow (Recommended)

```bash
# Interactive
./install.sh

# Non-interactive
./install.sh --api-url https://nest.example.com --generation gen2 --yes
```

### Option 2: Separate Steps

#### Step 1: Build Firmware

```bash
# Interactive
./build-firmware.sh

# Non-interactive
./build-firmware.sh --api-url https://nest.example.com --generation gen2 --yes

# With SSH access for debugging
./build-firmware.sh --api-url https://nest.example.com --enable-root-access --yes
```

#### Step 2: Flash Firmware

```bash
# Flash (builds omap_loader automatically if needed)
./flash-firmware.sh --generation gen2

# Just build omap_loader without flashing
./flash-firmware.sh --build-only
```

## Directory Structure

```
firmware-standalone/
├── install.sh               # Combined build + flash workflow
├── build-firmware.sh        # Build firmware (Docker)
├── flash-firmware.sh        # Flash firmware (USB DFU)
├── README.md
├── builder/                  # Firmware build system
│   ├── build.sh             # Main build script
│   ├── docker-build.sh      # Docker wrapper
│   ├── Dockerfile           # Build environment
│   ├── deps/                # Build dependencies
│   ├── scripts/             # Build helper scripts
│   └── firmware/            # Output firmware files
│       ├── uImage           # Kernel image
│       ├── u-boot.bin       # U-Boot bootloader
│       ├── x-load-gen1.bin  # Gen 1 first-stage bootloader
│       └── x-load-gen2.bin  # Gen 2 first-stage bootloader
├── omap_loader/              # USB flash tool
│   ├── src/                 # Source code
│   │   ├── omap_loader.c
│   │   └── Makefile
│   ├── patches/             # Platform-specific patches
│   └── bin/                 # Compiled binaries (by platform)
│       ├── macos-arm64/
│       ├── macos-x64/
│       └── linux-x64/
└── docs/                     # Documentation
```

## Build Options

| Option | Description |
|--------|-------------|
| `--api-url <url>` | Your self-hosted server URL (required) |
| `--generation <gen>` | `gen1`, `gen2`, or `both` (default: gen2) |
| `--custom-cert <path>` | Embed a custom CA certificate (PEM) instead of Mozilla's bundle |
| `--enable-root-access` | Enable SSH root login (password: nest) |
| `--force-build` | Force rebuild even if firmware exists |
| `--yes`, `-y` | Non-interactive mode |

## Flash Options

| Option | Description |
|--------|-------------|
| `--generation <gen>` | `gen1` or `gen2` (default: gen2) |
| `--build-only` | Only build omap_loader, don't flash |

## DFU Mode Instructions

To put your Nest thermostat in DFU (Device Firmware Upgrade) mode:

1. **Remove** thermostat from wall mount (pull straight out)
2. **Hold** the ring (don't press, just hold down)
3. **Connect** USB cable to computer while holding
4. **Wait** 10 seconds until screen goes blank
5. **Release** ring - device is now in DFU mode

The display will be black/blank. This is expected.

## Manual Flash Command

If you need to flash manually:

```bash
# Gen 2
sudo ./omap_loader/bin/macos-arm64/omap_loader \
  -f builder/firmware/x-load-gen2.bin \
  -f builder/firmware/u-boot.bin -a 0x80100000 \
  -f builder/firmware/uImage -a 0x80A00000 \
  -j 0x80100000

# Gen 1
sudo ./omap_loader/bin/macos-arm64/omap_loader \
  -f builder/firmware/x-load-gen1.bin \
  -f builder/firmware/u-boot.bin -a 0x80100000 \
  -f builder/firmware/uImage -a 0x80A00000 \
  -j 0x80100000
```

## Thermostat Generations

| Generation | Codename | Description |
|------------|----------|-------------|
| Gen 1 | Diamond | Original Nest Learning Thermostat |
| Gen 2 | j49 | Second generation (most common) |

## SSL/TLS Configuration

By default, this package uses **public SSL** mode with the Mozilla CA bundle, which is compatible with:
- Let's Encrypt certificates
- Any publicly-trusted SSL certificate
- Standard HTTPS setups

To use a **custom CA certificate** instead (e.g., self-signed or private CA):

```bash
./build-firmware.sh --api-url https://nest.example.com --custom-cert /path/to/my-ca.pem --yes
```

The certificate must be PEM-encoded. When `--custom-cert` is used, only that CA will be trusted by the device.

## Troubleshooting

### Docker Build Fails

```bash
# Ensure Docker is running
docker info

# Check Docker has enough resources
# Docker Desktop → Settings → Resources → Memory: 4GB+
```

### omap_loader Build Fails

```bash
# macOS: Install Xcode tools
xcode-select --install

# macOS: Install libusb
brew install libusb

# Linux: Install dependencies
sudo apt-get install build-essential libusb-1.0-0-dev
```

### Device Not Detected

```bash
# Check USB connection
# macOS
system_profiler SPUSBDataType | grep -A 10 "OMAP"

# Linux
lsusb | grep -i "0451:d00e"

# Device should show: ID 0451:d00e Texas Instruments
```

### Permission Denied

```bash
# omap_loader requires sudo for USB access
sudo ./flash-firmware.sh

# Or run manually with sudo
sudo ./omap_loader/bin/macos-arm64/omap_loader ...
```

### Flash Fails

1. Ensure device is in DFU mode (blank screen)
2. Try a different USB cable (data cable, not charge-only)
3. Try a different USB port (USB-A preferred over USB-C hubs)
4. Check system logs for USB errors

## Platform Support

| Platform | Build Firmware | Flash Firmware |
|----------|----------------|----------------|
| macOS (Apple Silicon) | ✅ | ✅ |
| macOS (Intel) | ✅ | ✅ |
| Linux (x64) | ✅ | ✅ |
| Windows | ✅ (WSL) | ❌ (use Linux/macOS) |

## Documentation

Additional documentation is available in the `docs/` folder:

- [Device Authentication](docs/device-authentication.md) - How Nest devices authenticate with the API server
- [Self-Hosted Proxy Setup](docs/self-hosted-proxy-setup.md) - Run a proxy server that forwards to an upstream backend
- [Self-Hosted Full Backend Setup](docs/self-hosted-full-backend-setup.md) - Run your own complete backend server

## License

- **Firmware tools** - MIT License
- **omap_loader** - GPLv2 (by grant-h/ajb142)
