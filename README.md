# Nest.Thermostat

> **⚠️ Work in Progress** — This project is under active development and may not fully work. No warranty is provided. Use at your own risk.

Local API server and custom firmware tools for Nest thermostat management. Allows Nest thermostats to connect to your own server instead of Google's cloud.

## Features

- **Proxy Mode**: Forwards requests to upstream Nest API with full traffic logging
- **Local Device API**: Device management, state tracking, scheduling, weather proxy
- **File-based Storage**: JSON documents stored on filesystem (no cloud dependencies)
- **In-memory Caching**: Fast device state access
- **Custom Firmware**: Build and flash modified firmware to redirect Nest thermostats to your server

## Projects

| Project | Description |
|---------|-------------|
| `Nest.Thermostat.Api` | ASP.NET Core Web API — main runtime host |
| `Nest.Thermostat.Core` | Core business logic, storage, models |
| `Nest.Thermostat.Tests` | Unit and integration tests |
| `firmware/` | Custom firmware build and flash tools |

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- (Optional) Nest thermostat for testing

### Build & Run

```bash
# Build
dotnet build

# Run API
dotnet run --project Nest.Thermostat.Api

# Run tests
dotnet test
```

### Configuration

Configure in `Nest.Thermostat.Api/appsettings.json`:

```json
{
  "Storage": {
    "BasePath": "./data"
  },
  "NestProxy": {
    "Enabled": true,
    "UpstreamBaseUrl": "https://nest.example.com",
    "AllowedSerials": ["*"]
  }
}
```

## API Endpoints

### Proxy Endpoints (for thermostat devices)

| Endpoint | Description |
|----------|-------------|
| `GET/POST /nest/entry` | Service discovery |
| `GET /nest/ping` | Health check |
| `GET /nest/passphrase` | Pairing passphrase |
| `POST /nest/transport` | Streaming subscribe |
| `POST /nest/transport/put` | State updates |
| `GET /nest/weather/v1` | Weather data |

### Device Management (for clients)

| Endpoint | Description |
|----------|-------------|
| `GET /devices` | List all devices |
| `GET /devices/{serial}` | Get device details |
| `POST /devices/{serial}/temperature` | Set target temperature |

## Storage Structure

```
./data/
├── devices/           # Device configurations
├── shared/            # Current device states
├── schedules/         # Heating/cooling schedules
├── structures/        # Home/structure data
├── users/             # User accounts
└── logs/              # Proxy traffic logs
    └── proxy/
        └── YYYY-MM-DD/
```

## Firmware

The `firmware/` directory contains tools to build and flash custom firmware for Nest Gen1 and Gen2 thermostats. The firmware redirects the thermostat to connect to your self-hosted server instead of Google/Nest cloud services.

### Quick Start

```bash
cd firmware

# Interactive build and flash
./install.sh

# Or step by step:
./build-firmware.sh --api-url https://nest.example.com --generation gen2 --yes
./flash-firmware.sh --generation gen2
```

### Requirements

- **Docker** for building firmware
- **libusb** for flashing via USB DFU mode (`brew install libusb` on macOS)

### Firmware Scripts

| Script | Purpose |
|--------|---------|
| `firmware/install.sh` | Combined build + flash workflow |
| `firmware/build-firmware.sh` | Build firmware image (requires Docker) |
| `firmware/flash-firmware.sh` | Flash firmware to device via USB DFU |

### DFU Mode

To flash, the thermostat must be in DFU mode:

1. Remove from wall mount
2. Hold the ring down
3. Connect USB cable while holding
4. Wait 10 seconds until screen goes blank
5. Release ring

See [firmware/README.md](firmware/README.md) for full details, build options, and troubleshooting.

## Credits

Based on work from [codykociemba/NoLongerEvil-Thermostat](https://github.com/codykociemba/NoLongerEvil-Thermostat) (branch: `open-source-prototype`).

## License

MIT
