# Nest Gen2 Device Authentication

This document explains how Nest Gen2 thermostats authenticate with the Nest Thermostat API (or original Google/Nest servers).

## Overview

The Nest Gen2 thermostat uses a **multi-layered authentication approach** combining:

1. **TLS Certificate Validation** - Server identity verification
2. **HTTP Basic Authentication** - Device identity via serial number
3. **Entry Key Pairing** - User-to-device association

---

## 1. TLS/SSL Certificate Authentication

### How It Works

The Nest thermostat validates the server's SSL certificate before establishing any connection. The firmware contains a **CA certificate bundle** that must include the server's certificate authority.

### Public SSL Setup (Recommended)

When building firmware with `--public-ssl`, Mozilla's full CA bundle is embedded, enabling the device to trust any publicly-trusted certificate (Let's Encrypt, DigiCert, etc.). This is the recommended approach for self-hosted deployments.

### Custom CA Certificate Setup

For deployments using a private or self-signed CA, use `--custom-cert` to embed your own certificate:

```bash
./build-firmware.sh \
  --api-url https://nest.example.com \
  --custom-cert /path/to/my-ca.pem \
  --yes
```

The certificate must be PEM-encoded. Only this CA (and the certificates it has signed) will be trusted by the device.

### Legacy Custom CA Setup

For self-hosted deployments with a custom CA:

1. Generate your own CA certificate
2. Place it at `/server/certs/ca-cert.pem` (auto-detected by the build when `--public-ssl` is not used)
3. Configure the server with matching TLS certificates

The device will **reject connections** to servers that don't present a certificate signed by the embedded CA.

---

## 2. HTTP Basic Authentication

### How It Works

Every request from the device includes an `Authorization` header using HTTP Basic Authentication:

```
Authorization: Basic base64(serial:password)
```

The username field contains the device serial number, optionally prefixed with `nest.`:

```
Authorization: Basic bmVzdC5BQkNERTEyMzQ1Njc4OQ==
                      ^--- base64("nest.ABCDE123456789:password")
```

### Server-Side Extraction

The server extracts the serial from the Basic Auth header in `NestProxyController.ResolveDeviceSerial()`:

```csharp
private string? ResolveDeviceSerial()
{
    if (Request.Headers.TryGetValue("X-NL-Device-Serial", out var serialHeader))
    {
        return serialHeader.FirstOrDefault();
    }

    if (Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var auth = authHeader.FirstOrDefault();
        if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var encoded = auth["Basic ".Length..];
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':');
            if (parts.Length >= 1 && !string.IsNullOrEmpty(parts[0]))
            {
                var username = parts[0];
                if (username.StartsWith("nest.", StringComparison.OrdinalIgnoreCase))
                {
                    username = username["nest.".Length..];
                }
                return username;
            }
        }
    }

    return null;
}
```

### Fallback: Custom Header

If Basic Auth is not present, the server checks for a custom header:

```
X-NL-Device-Serial: ABCDE123456789
```

### Serial Validation

Serial numbers are:
- Uppercased
- Stripped of non-alphanumeric characters
- Required to be at least 10 characters

---

## 3. Entry Key Pairing System

### Purpose

The entry key system associates a **device** with a **user account**. This is the "pairing code" displayed on the thermostat screen.

### Entry Key Format

A 7-character code consisting of:
- 3 digits (000-999)
- 4 uppercase letters (A-Z)

Example: `123ABCD`

### Generation Flow

```
┌─────────────────┐    GET /nest/passphrase    ┌─────────────────┐
│                 │ ─────────────────────────► │                 │
│  Nest Device    │                            │  Nest Server    │
│                 │ ◄───────────────────────── │                 │
└─────────────────┘   { value: "123ABCD",      └─────────────────┘
                        expires: 1735084800000 }
```

1. Device requests a passphrase via `GET /nest/passphrase`
2. Server generates a unique 7-character code
3. Server stores the code with:
   - Device serial number
   - Creation timestamp
   - Expiration timestamp (default: 1 hour)
4. Device displays the code on screen

### Pairing Process

1. User enters the 7-character code in the web app or mobile app
2. Server looks up the code
3. If valid and not expired:
   - Server marks the code as claimed by the user
   - Server creates a device ownership record linking user → device
4. Future requests from the device are now associated with that user

---

## 4. Control API Authentication (External Apps)

### API Keys

For external applications (dashboards, home automation), the server supports **API Key authentication**:

```
Authorization: Bearer nlapi_xxxxxxxxxxxx
```

### API Key Format

Keys are prefixed with `nlapi_` for identification.

### API Key Permissions

Keys include scoped permissions:
- `serials[]` - List of device serials the key can access
- `scopes[]` - Allowed operations (e.g., `read`, `write`, `control`)

---

## Authentication Flow Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        DEVICE AUTHENTICATION FLOW                        │
└──────────────────────────────────────────────────────────────────────────┘

    NEST DEVICE                                           NEST SERVER
    ───────────                                           ───────────
         │                                                     │
         │  1. TLS Handshake                                   │
         │  ─────────────────────────────────────────────────► │
         │     (Validates server cert against embedded CA)     │
         │                                                     │
         │  2. GET /nest/entry                                 │
         │     Authorization: Basic base64(nest.SERIAL:pass)   │
         │  ─────────────────────────────────────────────────► │
         │                                                     │
         │     { transport_url, passphrase_url, ... }          │
         │  ◄───────────────────────────────────────────────── │
         │                                                     │
         │  3. GET /nest/passphrase                            │
         │     Authorization: Basic base64(nest.SERIAL:pass)   │
         │  ─────────────────────────────────────────────────► │
         │                                                     │
         │     { value: "123ABCD", expires: ... }              │
         │  ◄───────────────────────────────────────────────── │
         │                                                     │
         │  (Device displays "123ABCD" on screen)              │
         │                                                     │
    ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─
         │                                                     │
    USER │  4. User enters "123ABCD" in web/mobile app         │
         │  ─────────────────────────────────────────────────► │
         │                                                     │
         │     Server links device to user account             │
         │                                                     │
    ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─ ─
         │                                                     │
         │  5. POST /nest/transport                            │
         │     (Subscribe to state changes)                    │
         │  ─────────────────────────────────────────────────► │
         │                                                     │
         │     { objects: [...state...] }                      │
         │  ◄───────────────────────────────────────────────── │
         │                                                     │
```

---

## Security Considerations

### What the Device Authenticates

| Check | Description |
|-------|-------------|
| ✅ Server Certificate | Device validates server's TLS cert against embedded CA |
| ✅ Device Identity | Serial sent via Basic Auth on every request |
| ❌ Device Secret | No unique per-device cryptographic secret |

### Implications

1. **Device Impersonation**: If an attacker knows a device's serial number, they could potentially impersonate it. The entry key pairing system mitigates this by requiring physical access to the device (to see the displayed code).

2. **Man-in-the-Middle**: The device validates server certificates against the embedded CA bundle, preventing MitM attacks.

3. **Self-Hosted Security**: When self-hosting, you control the CA certificate, providing complete trust isolation.

---

## Comparison: Google vs Self-Hosted

| Aspect | Google/Nest (Original) | Self-Hosted (This Project) |
|--------|----------------------|----------------------------|
| TLS | Google PKI | Public SSL (Mozilla CA) or Custom CA |
| Device Auth | Basic Auth + Google OAuth | Basic Auth (serial number) |
| User Auth | Google Account | Entry Key pairing |
| API Access | Nest API (deprecated) | Local REST API |
| Data Storage | Google Cloud | File-based JSON documents |

---

## Related Files

- [Nest.Thermostat.Api/Controllers/NestProxyController.cs](../Nest.Thermostat.Api/Controllers/NestProxyController.cs) - Serial extraction, proxy endpoints
- [Nest.Thermostat.Api/Services/NestProxyService.cs](../Nest.Thermostat.Api/Services/NestProxyService.cs) - Upstream proxy service
- [Nest.Thermostat.Api/Configuration/NestApiSettings.cs](../Nest.Thermostat.Api/Configuration/NestApiSettings.cs) - API origin and entry key settings
- [Nest.Thermostat.Api/Configuration/NestProxySettings.cs](../Nest.Thermostat.Api/Configuration/NestProxySettings.cs) - Proxy and serial allowlist settings
- [firmware/builder/build.sh](../firmware/builder/build.sh) - Certificate embedding during firmware build
