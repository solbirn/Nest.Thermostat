# Self-Hosted Proxy Setup Guide

This guide explains how to set up your own **proxy server** for Nest thermostats. In proxy mode, your server acts as a man-in-the-middle between the thermostat and the upstream Nest backend, giving you full visibility into device communication while keeping the original backend functional.

## Overview

```
┌─────────────────────┐     HTTPS      ┌──────────────────────────┐     HTTPS     ┌──────────────────────┐
│   Nest Thermostat   │ ─────────────► │  Nest.Thermostat.Api     │ ────────────► │  Upstream Backend    │
│   (custom firmware) │                │  nest.example.com        │               │  (Nest servers)      │
└─────────────────────┘                │  Your Server / VPS       │               └──────────────────────┘
                                       │  + Let's Encrypt SSL     │
                                       └──────────────────────────┘
```

**Key Benefits:**
- Standard publicly-trusted SSL (e.g., Let's Encrypt)
- Full logging of thermostat ↔ backend communication
- Device serial allowlisting
- Rate limiting per endpoint
- Never need to reflash firmware for certificate renewal

**Proxy vs Full Backend:**

| Feature | Proxy Mode | Full Backend Mode |
|---------|-----------|------------------|
| Upstream dependency | Yes (forwards to Nest servers) | No (fully standalone) |
| Data storage | Optional (logging only) | Required (file-based JSON) |
| Configuration | `NestProxy.Enabled = true` | `NestProxy.Enabled = false` |
| Use case | Monitoring, debugging, research | Complete independence |

See [self-hosted-full-backend-setup.md](self-hosted-full-backend-setup.md) for standalone mode.

## Prerequisites

- Docker installed (for firmware building)
- A server capable of running .NET (VPS, cloud VM, home server, etc.)
- Custom domain (e.g., `nest.example.com`)
- Access to DNS configuration
- .NET 10 SDK (for local development)

---

## Part 1: Build Custom Firmware

### 1.1 Understanding the Certificate Trust Chain

The stock Nest firmware only trusts 3 certificates:
- Nest Private Root Certificate Authority (Nest's internal CA)
- Go Daddy Class 2 Certification Authority
- ValiCert Class 2 Policy Validation Authority

**Problem:** Modern public CAs (Let's Encrypt, DigiCert, etc.) are NOT trusted by stock firmware.

**Solution:** The custom firmware build embeds Mozilla's full CA bundle, enabling trust for all standard public CAs.

### 1.2 Build Firmware

```bash
cd firmware

# Build with your custom domain
./build-firmware.sh \
  --api-url https://nest.example.com \
  --generation gen2 \
  --yes
```

The build script automatically includes `--public-ssl` support, which downloads and embeds Mozilla's CA bundle (~250KB) into the firmware's `/etc/ssl/certs/ca-bundle.pem`.

To use your own CA certificate instead:

```bash
./build-firmware.sh \
  --api-url https://nest.example.com \
  --custom-cert /path/to/my-ca.pem \
  --generation gen2 \
  --yes
```

**Build output:**
- `builder/firmware/uImage` — Kernel with Mozilla CA bundle
- `builder/firmware/u-boot.bin` — Bootloader
- `builder/firmware/x-load-gen2.bin` — X-loader for Gen2

### 1.3 Flash the Firmware

```bash
./flash-firmware.sh --generation gen2
```

See [firmware/README.md](../firmware/README.md) for DFU mode instructions.

---

## Part 2: Configure Proxy Mode

The proxy controller and service already exist in the codebase. You just need to configure them.

### 2.1 Key Files

| File | Purpose |
|------|---------|
| [NestProxyController.cs](../Nest.Thermostat.Api/Controllers/NestProxyController.cs) | Handles all `/nest/*` device endpoints |
| [NestProxyService.cs](../Nest.Thermostat.Api/Services/NestProxyService.cs) | Proxies requests to upstream backend with logging |
| [NestProxySettings.cs](../Nest.Thermostat.Api/Configuration/NestProxySettings.cs) | Proxy configuration model |
| [FileLoggingService.cs](../Nest.Thermostat.Api/Infrastructure/FileLoggingService.cs) | Writes proxy traffic logs to disk |

### 2.2 Configure appsettings.json

Edit `Nest.Thermostat.Api/appsettings.json`:

```json
{
  "Storage": {
    "BasePath": "./data",
    "IndentJson": true
  },
  "NestProxy": {
    "Enabled": true,
    "UpstreamBaseUrl": "https://frontdoor.nest.com",
    "TimeoutSeconds": 120,
    "EnableLogging": true,
    "LogToFile": true,
    "LogToConsole": false,
    "LogBody": false,
    "AllowedSerials": "*"
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

### 2.3 Configuration Reference

#### NestProxy Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | `true` | Kill switch — when `false`, all proxy requests return 503 |
| `UpstreamBaseUrl` | string | `https://frontdoor.nest.com` | Upstream Nest API URL |
| `TimeoutSeconds` | int | `120` | Timeout for upstream requests |
| `EnableLogging` | bool | `true` | Enable proxy traffic logging |
| `LogToFile` | bool | `true` | Write traffic logs to disk |
| `LogToConsole` | bool | `false` | Write traffic logs to console |
| `LogBody` | bool | `false` | Include request/response bodies in logs |
| `MaxLogBodySize` | int | `100000` | Max body size to log (characters) |
| `AllowedSerials` | string | `*` | Comma-separated allowlist, or `*` for all |

#### NestApi Settings

| Setting | Type | Description |
|---------|------|-------------|
| `Origin` | string | Your custom domain (used in entry response URLs) |
| `EntryKeyTtlSeconds` | int | Entry key TTL for device pairing |
| `WeatherCacheTtlMs` | int | Weather data cache duration |
| `ServerVersion` | string | Version reported to devices |
| `TierName` | string | Tier name reported to devices |

---

## Part 3: How the Proxy Works

### 3.1 Request Flow

1. **Device boots** → calls `GET /nest/entry`
2. **`NestProxyController.GetEntry()`** returns service discovery URLs pointing to your domain
3. **Device calls** `/nest/transport` (POST) to subscribe to state updates
4. **`NestProxyService.InitiateStreamingRequestAsync()`** opens a streaming connection to the upstream backend
5. **Response is streamed** line-by-line back to the device via chunked transfer encoding
6. **Device calls** `/nest/transport/put` to report state changes
7. **`NestProxyService.ProxyRequestAsync()`** forwards the request and logs traffic

### 3.2 Device Serial Resolution

The controller extracts the device serial from requests using `ResolveDeviceSerial()`:

```csharp
// 1. Check X-NL-Device-Serial header
if (Request.Headers.TryGetValue("X-NL-Device-Serial", out var serialHeader))
    return serialHeader.FirstOrDefault();

// 2. Parse from Basic auth: "nest.SERIAL:password" → "SERIAL"
var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
var username = decoded.Split(':')[0];
if (username.StartsWith("nest.", StringComparison.OrdinalIgnoreCase))
    username = username["nest.".Length..];
```

### 3.3 Serial Allowlisting

The `AllowedSerials` setting controls which devices can use the proxy:

- `"*"` or empty — all devices allowed
- `"SERIAL1,SERIAL2"` — only listed serials allowed
- Unauthorized devices receive `403 Forbidden`

### 3.4 Traffic Logging

When `LogToFile` is enabled, proxy traffic is logged to:

```
./data/logs/proxy/YYYY-MM-DD/
├── GET_nest-entry_20250101T120000_a1b2c3d4.json
├── POST_nest-transport_20250101T120001_e5f6g7h8.json
└── ...
```

Each log file contains the request method, path, headers, and optionally the body (when `LogBody` is `true`).

### 3.5 Rate Limiting

Each endpoint has rate limits applied via the `[RateLimit]` attribute:

| Endpoint | Max Requests | Window |
|----------|-------------|--------|
| `/nest/entry` | 60/min | Per device |
| `/nest/ping` | 120/min | Per device |
| `/nest/passphrase` | 30/min | Per device |
| `/nest/transport` | 30/min | Per device |
| `/nest/transport/put` | 60/min | Per device |

---

## Part 4: Deploy

### 4.1 Build and Publish

```bash
cd Nest.Thermostat.Api
dotnet publish -c Release -o ./publish
```

### 4.2 Deploy to Your Server

Copy the published output to your server and run it:

```bash
cd /opt/nest-thermostat
dotnet Nest.Thermostat.Api.dll
```

Or run as a systemd service (see [full backend setup](self-hosted-full-backend-setup.md) for a service file example).

### 4.3 Configure DNS and SSL

1. **Point your domain** to your server:

   | Type | Name | Value |
   |------|------|-------|
   | A    | nest | `<your-server-ip>` |

2. **Set up a reverse proxy** with automatic HTTPS (Caddy, nginx + certbot, etc.). See the [full backend setup guide](self-hosted-full-backend-setup.md#part-4-custom-domain-and-ssl) for detailed examples.

3. **Important for long-polling:** Configure your reverse proxy with a long read timeout (10+ minutes) to support `/nest/transport` subscriptions.

---

## Part 5: Testing

### 5.1 Test Proxy Endpoints

```bash
# Test entry endpoint (returns service discovery URLs)
curl -v https://nest.example.com/nest/entry

# Test ping
curl -v https://nest.example.com/nest/ping

# Test with device serial
curl -v https://nest.example.com/nest/passphrase \
  -H "Authorization: Basic $(echo -n 'nest.TEST123:password' | base64)"

# Test weather
curl -v "https://nest.example.com/nest/weather/v1?query=94301,US"
```

### 5.2 Verify SSL Certificate

```bash
openssl s_client -connect nest.example.com:443 -servername nest.example.com
# Should show Let's Encrypt certificate chain
```

### 5.3 Monitor Logs

```bash
# Stream application logs
journalctl -u nest-thermostat -f

# Check proxy traffic logs
ls ./data/logs/proxy/$(date +%Y-%m-%d)/
```

---

## Part 6: Troubleshooting

### Device Not Connecting

**Symptom:** Thermostat doesn't contact proxy after flashing.

1. Verify firmware was built with `--api-url https://nest.example.com`
2. Check DNS: `nslookup nest.example.com`
3. Test HTTPS: `curl -v https://nest.example.com/nest/ping`

### SSL Certificate Errors

**Symptom:** SSL handshake failures.

1. Verify SSL certificate is valid and bound to domain
2. Verify firmware includes the correct CA (Mozilla bundle via `--public-ssl`, or your cert via `--custom-cert`)
3. Check: `openssl s_client -connect nest.example.com:443`
4. Renew if expired: `sudo certbot renew`

### 502 Bad Gateway

**Symptom:** Proxy returns 502 errors.

1. Check upstream availability: `curl -v https://frontdoor.nest.com/nest/ping`
2. Check `NestProxy.UpstreamBaseUrl` in appsettings
3. Review logs for exception details

### 503 Service Unavailable

**Symptom:** All requests return 503.

1. Check `NestProxy.Enabled` is `true` in appsettings
2. Restart the application after config changes

### 403 Forbidden

**Symptom:** Device gets 403 response.

1. Check `NestProxy.AllowedSerials` includes the device serial (or is `"*"`)
2. Verify the device serial is being sent via `X-NL-Device-Serial` header or Basic auth

### Long-Poll Timeout

**Symptom:** `/nest/transport` subscriptions disconnect early.

1. Increase `NestProxy.TimeoutSeconds` (default: 120)
2. Increase your reverse proxy's read timeout (e.g., `proxy_read_timeout 600s` in nginx)
3. Ensure your server process stays running (systemd, Docker, etc.)

---

## Summary

| Component | Configuration |
|-----------|---------------|
| **Firmware** | Custom build with `--api-url https://your-domain.com` |
| **CA Bundle** | Mozilla's full CA bundle (~250KB) |
| **SSL** | Let's Encrypt (via Caddy, certbot, etc.) |
| **Proxy Controller** | `NestProxyController` — handles all `/nest/*` routes |
| **Proxy Service** | `NestProxyService` — forwards to upstream with logging |
| **Upstream** | Configurable via `NestProxy.UpstreamBaseUrl` |
| **Logging** | File-based traffic logs in `./data/logs/proxy/` |

**This setup provides:**
- No certificate management or renewal
- No firmware reflashing for certificate updates
- Full traffic logging and monitoring
- Per-device serial allowlisting
- Rate limiting per endpoint
- Hot-reloadable configuration via `IOptionsMonitor`
