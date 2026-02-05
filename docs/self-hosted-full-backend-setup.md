# Self-Hosted Server Setup Guide (Full Backend)

This guide explains how to run your own **complete Nest backend server** using the ASP.NET Core implementation, eliminating any dependency on external services.

## Overview

```
┌─────────────────────┐     HTTPS      ┌──────────────────────────────────────┐
│   Nest Thermostat   │ ─────────────► │  Nest.Thermostat.Api                 │
│   (custom firmware) │                │  nest.example.com                    │
└─────────────────────┘                │  Your Server / VPS / Cloud           │
                                       │  + File-based Storage                │
                                       └──────────────────────────────────────┘
```

**Key Benefits:**
- Full control over your thermostat data and backend
- No dependency on any external service
- Data stored as JSON files on disk (or your own database)
- Standard publicly-trusted SSL certificate (e.g., Let's Encrypt)
- Never need to reflash firmware for certificate renewal

## Prerequisites

- Docker installed (for firmware building)
- A server capable of running .NET (VPS, cloud VM, home server, etc.)
- Custom domain (e.g., `nest.example.com`)
- Access to DNS configuration
- .NET 10 SDK (for local development)

---

## Part 1: Server Setup

### 1.1 Prepare Your Server

You can host the backend on any server that supports .NET 10:
- **VPS** (DigitalOcean, Linode, Hetzner, etc.)
- **Cloud VM** (any cloud provider)
- **Home server** or Raspberry Pi
- **Container** via Docker

**Requirements:**
- .NET 10 runtime installed
- Port 443 (HTTPS) accessible
- Reverse proxy (nginx, Caddy, etc.) recommended for TLS termination

---

## Part 2: Application Configuration

### 2.1 Update appsettings.json

Modify the configuration in `Nest.Thermostat.Api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Storage": {
    "BasePath": "./data",
    "IndentJson": true
  },
  "NestProxy": {
    "Enabled": false
  },
  "NestApi": {
    "Origin": "https://nest.example.com",
    "EntryKeyTtlSeconds": 3600,
    "WeatherCacheTtlMs": 600000,
    "ServerVersion": "1.0.0",
    "TierName": "production"
  }
}
```

**Required changes:**

| Setting | Description | Example |
|---------|-------------|---------|
| `NestApi.Origin` | Your custom domain (HTTPS) | `https://nest.example.com` |
| `NestProxy.Enabled` | Set to `false` for standalone mode | `false` |
| `Storage.BasePath` | Path to data directory | `./data` |

> **Note:** When running in standalone (non-proxy) mode, set `NestProxy.Enabled` to `false`. The server will handle all device requests locally instead of forwarding to an upstream backend.

---

## Part 3: Deploy Application

### 3.1 Build and Publish

```bash
cd Nest.Thermostat.Api

# Restore dependencies
dotnet restore

# Build release
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish
```

### 3.2 Deploy to Your Server

Copy the published output to your server and run it:

```bash
# On your server
cd /opt/nest-thermostat
dotnet Nest.Thermostat.Api.dll
```

Or run as a systemd service:

```ini
# /etc/systemd/system/nest-thermostat.service
[Unit]
Description=Nest Thermostat API
After=network.target

[Service]
WorkingDirectory=/opt/nest-thermostat
ExecStart=/usr/bin/dotnet /opt/nest-thermostat/Nest.Thermostat.Api.dll
Restart=always
RestartSec=10
User=nest
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable nest-thermostat
sudo systemctl start nest-thermostat
```

---

## Part 4: Custom Domain and SSL

### 4.1 Configure DNS

Point your domain to your server:

| Type | Name | Value |
|------|------|-------|
| A    | nest | `<your-server-ip>` |

### 4.2 Set Up SSL with Let's Encrypt

Using **Caddy** (automatic HTTPS):

```
# /etc/caddy/Caddyfile
nest.example.com {
    reverse_proxy localhost:5000
}
```

Or using **nginx** + **certbot**:

```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d nest.example.com
```

```nginx
# /etc/nginx/sites-available/nest-thermostat
server {
    listen 443 ssl;
    server_name nest.example.com;

    ssl_certificate /etc/letsencrypt/live/nest.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/nest.example.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 600s;  # Long timeout for /nest/transport
    }
}
```

Certbot auto-renews certificates, so firmware never needs reflashing.

---

## Part 5: Firmware Build and Flash

### 5.1 Build Custom Firmware

The firmware must include a CA bundle that trusts your server's SSL certificate.

For publicly-trusted certificates (Let's Encrypt, etc.), the default build uses Mozilla's CA bundle:

```bash
cd firmware

# Build with public SSL support (default)
./build-firmware.sh \
  --api-url https://nest.example.com \
  --generation gen2 \
  --yes
```

For a private or self-signed CA, use `--custom-cert` instead:

```bash
./build-firmware.sh \
  --api-url https://nest.example.com \
  --custom-cert /path/to/my-ca.pem \
  --generation gen2 \
  --yes
```

**Build output:**
- `builder/firmware/uImage` - Kernel with Mozilla CA bundle
- `builder/firmware/u-boot.bin` - Bootloader
- `builder/firmware/x-load-gen2.bin` - X-loader for Gen2

### 5.2 Flash the Firmware

```bash
./flash-firmware.sh --generation gen2
```

See [firmware/README.md](../firmware/README.md) for DFU mode instructions and troubleshooting.

---

## Part 6: API Reference

### Device API (Nest Thermostat Communication)

| Route | Method | Description |
|-------|--------|-------------|
| `/nest/entry` | GET/POST | Service discovery - returns transport URLs |
| `/nest/ping` | GET | Health check endpoint |
| `/nest/passphrase` | GET | Generate/retrieve device pairing code |
| `/nest/pro_info` | GET | Installer information lookup |
| `/nest/upload` | POST | Device log upload |
| `/nest/weather/v1` | GET | Weather data (with caching) |
| `/nest/transport/device/{serial}` | GET | Get device state objects |
| `/nest/transport` | POST | Subscribe to state updates (long-poll) |
| `/nest/transport/put` | POST | Submit device state updates |

### Device Management API

| Route | Method | Description |
|-------|--------|-------------|
| `/devices` | GET | List all devices |
| `/devices/{serial}` | GET | Get device details and state |
| `/devices/{serial}/{objectType}` | GET | Get specific device object |
| `/devices/{serial}` | DELETE | Delete device and all data |
| `/health` | GET | Server health check |

### Entry Response Example

When the thermostat calls `/nest/entry`, it receives:

```json
{
  "czfe_url": "https://nest.example.com/nest/transport",
  "transport_url": "https://nest.example.com/nest/transport",
  "direct_transport_url": "https://nest.example.com/nest/transport",
  "passphrase_url": "https://nest.example.com/nest/passphrase",
  "ping_url": "https://nest.example.com/nest/ping",
  "pro_info_url": "https://nest.example.com/nest/pro_info",
  "weather_url": "https://nest.example.com/nest/weather/v1?query=",
  "server_version": "1.0.0",
  "tier_name": "production"
}
```

---

## Part 7: Testing

### 7.1 Test API Endpoints

```bash
# Test health
curl -v https://nest.example.com/health

# Test entry endpoint
curl -v https://nest.example.com/nest/entry

# Test ping
curl -v https://nest.example.com/nest/ping

# Test passphrase (requires device auth)
curl -v https://nest.example.com/nest/passphrase \
  -H "Authorization: Basic $(echo -n 'TEST-SERIAL:password' | base64)"

# Test weather
curl -v "https://nest.example.com/nest/weather/v1?query=94301,US"
```

### 7.2 Test Device Management API

```bash
# List devices
curl -v https://nest.example.com/devices

# Get device details
curl -v https://nest.example.com/devices/YOUR-DEVICE-SERIAL
```

---

## Part 8: Monitoring and Troubleshooting

### 8.1 View Logs

```bash
# Stream live logs via systemd
journalctl -u nest-thermostat -f

# Or check the application log files
tail -f /opt/nest-thermostat/logs/*.log
```

### 8.2 Common Issues

#### Device Not Connecting

**Symptom:** Thermostat doesn't contact server after flashing.

**Solutions:**
1. Verify firmware was built with correct `--api-url`
2. Check DNS resolution: `nslookup nest.example.com`
3. Test HTTPS: `curl -v https://nest.example.com/nest/ping`
4. Verify firmware CA matches your server's cert (public SSL or `--custom-cert`)

#### SSL Certificate Errors

**Symptom:** SSL handshake failures in logs.

**Solutions:**
1. Verify SSL certificate is valid and bound to domain
2. Check certificate validity: `openssl s_client -connect nest.example.com:443`
3. Renew if expired: `sudo certbot renew`

---

## Part 9: Architecture

### Data Flow

```
1. Device boots → calls /nest/entry
2. Server returns transport URLs with your domain
3. Device calls /nest/transport (POST) to subscribe
4. Server streams state updates via chunked transfer
5. Device calls /nest/transport/put to report state changes
6. Server persists state to file-based storage
```

### Storage Structure

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

### Key Services

| Service | Purpose |
|---------|---------|
| `NestProxyController` | Handles all `/nest/*` device endpoints |
| `NestProxyService` | Proxies requests to upstream (when enabled) |
| `DeviceRepository` | Device data access via file store |
| `FileDocumentStore` | JSON document persistence |
| `InMemoryCache` | Fast device state access |

---

## Summary

| Component | Configuration |
|-----------|---------------|
| **Backend** | Nest.Thermostat.Api (.NET 10) |
| **Storage** | File-based JSON documents |
| **Hosting** | Any server running .NET 10 (VPS, cloud VM, home server) |
| **SSL Certificate** | Let's Encrypt (via Caddy, certbot, etc.) |
| **Firmware** | Built with `--api-url https://your-domain.com` |
| **CA Bundle** | Mozilla's full CA bundle (~250KB) |

**This setup provides:**
- ✅ Complete backend independence (no external dependencies)
- ✅ Your data stays on your server
- ✅ No certificate management or renewal
- ✅ No firmware reflashing for certificate updates
- ✅ Full API access for home automation integration
- ✅ Swagger UI for API exploration (`/swagger` in development)
