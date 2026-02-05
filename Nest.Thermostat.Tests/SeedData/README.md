# Nest Thermostat Test Device Seed Data

This directory contains seed data for the Nest Thermostat integration tests.

## File-Based Storage

The seed data is stored as JSON and can be imported into the file-based storage:

```bash
# Create the data directory structure
mkdir -p ./data/{device-state,device-ownership,entry-key}

# Copy individual documents to appropriate directories
# or use the import script
```

## Seed Documents

### 1. Device State - shared.09AA01ACTEST

Located at: `data/device-state/09AA01ACTEST_shared_09AA01ACTEST.json`

### 2. Device State - device.09AA01ACTEST

Located at: `data/device-state/09AA01ACTEST_device_09AA01ACTEST.json`

### 3. Device Ownership

Located at: `data/device-ownership/09AA01ACTEST.json`

### 4. Entry Key (for testing claim flow)

Located at: `data/entry-key/456CLAIM.json`

## Test Constants

Use `TestDeviceConstants` class in tests:

```csharp
using Nest.Thermostat.Tests.Infrastructure;

// Serial: TestDeviceConstants.Serial        → "09AA01ACTEST"
// UserId: TestDeviceConstants.UserId        → "00000000-0000-0000-0000-000000000001"
// EntryKey: TestDeviceConstants.EntryKey    → "123TEST1"
```
