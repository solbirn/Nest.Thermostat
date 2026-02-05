# Nest API Object Value Schemas

Documentation of the `value` field structures for each object type in the Nest transport API.

---

## Object Types

| Object Key Pattern | Description |
|-------------------|-------------|
| `device.{serial}` | Device configuration and capabilities |
| `shared.{serial}` | Shared thermostat state (temperatures, HVAC states) |
| `schedule.{serial}` | Heating/cooling schedule |
| `structure.{id}` | Home/structure information |
| `user.{id}` | User account information |
| `link.{serial}` | Links device to structure |
| `device_alert_dialog.{serial}` | Alert dialog state |

---

## device.{serial}

Device configuration, capabilities, and settings.

```json
{
  "eco": {
    "mode": "schedule",
    "touched_by": 5,
    "touched_user_id": "user.1015974",
    "mode_update_timestamp": 1750990365
  },
  "leaf": true,
  "rssi": 72,
  "pro_id": "",
  "has_fan": true,
  "y2_type": "unknown",
  "fan_mode": "auto",
  "local_ip": "192.168.7.20",
  "tou_icon": false,
  "where_id": "00000000-0000-0000-0000-000100000006",
  "hvac_pins": "W1,Y1,Rh,G",
  "star_type": "unknown",
  "error_code": "",
  "forced_air": true,
  "hvac_wires": "Heat,Cool,Fan,Rh",
  "touched_by": {},
  "click_sound": "off",
  "has_x2_cool": false,
  "has_x2_heat": false,
  "has_x3_cool": false,
  "has_x3_heat": false,
  "mac_address": "18b430119e95",
  "postal_code": "07719",
  "rcs_capable": false,
  "country_code": "US",
  "has_alt_heat": false,
  "has_aux_heat": false,
  "safety_state": "none",
  "structure_id": "ddcadb6a-86d3-47c6-b177-847e3a891f87",
  "wiring_error": "",
  "battery_level": 3.777,
  "device_locale": "en_US",
  "has_dual_fuel": false,
  "has_emer_heat": false,
  "has_heat_pump": false,
  "heater_source": "gas",
  "leaf_away_low": 17.03693,
  "learning_mode": false,
  "model_version": "Display-2.12",
  "serial_number": "02AA01AC33140F61",
  "cooling_source": "electric",
  "fan_duty_cycle": 1800,
  "has_air_filter": true,
  "has_humidifier": false,
  "heat_x2_source": "gas",
  "heat_x3_source": "gas",
  "heatpump_ready": false,
  "leaf_away_high": 28.87999,
  "nlclient_state": "",
  "ob_orientation": "O",
  "ob_persistence": true,
  "time_to_target": 0,
  "alt_heat_source": "gas",
  "auto_away_reset": false,
  "aux_heat_source": "electric",
  "backplate_model": "Backplate-2.8",
  "current_version": "5.9.4-5",
  "fan_timer_speed": "stage1",
  "has_fossil_fuel": true,
  "has_x2_alt_heat": false,
  "heater_delivery": "forced-air",
  "home_away_input": true,
  "humidifier_type": "unknown",
  "target_humidity": 35,
  "auto_away_enable": false,
  "auto_dehum_state": false,
  "aux_lockout_leaf": 10,
  "capability_level": 5.94,
  "cooling_delivery": "unknown",
  "current_humidity": 44,
  "emer_heat_enable": false,
  "emer_heat_source": "electric",
  "fan_capabilities": "stage1",
  "has_dehumidifier": false,
  "heat_x2_delivery": "forced-air",
  "heat_x3_delivery": "forced-air",
  "heatpump_savings": "off",
  "hot_water_active": false,
  "humidifier_state": false,
  "logging_priority": "informational",
  "maint_band_lower": 0.39,
  "maint_band_upper": 0.39,
  "temperature_lock": false,
  "alt_heat_delivery": "forced-air",
  "aux_heat_delivery": "forced-air",
  "available_locales": "en_US,fr_CA,es_US,en_GB,fr_FR,nl_NL,es_ES,it_IT,de_DE",
  "cooling_x2_source": "electric",
  "cooling_x3_source": "electric",
  "dehumidifier_type": "unknown",
  "fan_control_state": false,
  "fan_cooling_state": false,
  "fan_current_speed": "off",
  "fan_duty_end_time": 64800,
  "fan_timer_timeout": 0,
  "lower_safety_temp": 7.2222,
  "pin_c_description": "none",
  "pin_g_description": "fan",
  "safety_state_time": 0,
  "temperature_scale": "F",
  "upper_safety_temp": 35,
  "alt_heat_x2_source": "gas",
  "auto_dehum_enabled": false,
  "backplate_bsl_info": "BSL",
  "dehumidifier_state": false,
  "demand_charge_icon": false,
  "emer_heat_delivery": "forced-air",
  "fan_schedule_speed": "stage1",
  "fan_timer_duration": 900,
  "filter_runtime_sec": 1709430,
  "gear_threshold_low": 0,
  "oob_temp_completed": true,
  "oob_test_completed": true,
  "oob_wifi_completed": true,
  "pin_ob_description": "none",
  "pin_rc_description": "none",
  "pin_rh_description": "power",
  "pin_w1_description": "heat",
  "pin_y1_description": "cool",
  "pin_y2_description": "none",
  "backplate_mono_info": "TFE (BP_D2) 4.2.8 (jenkins-slave@jenkins-agent-039-v17-emb-prod) 2019-04-03 17:36:54",
  "cooling_x2_delivery": "unknown",
  "cooling_x3_delivery": "unknown",
  "fan_cooling_enabled": true,
  "fan_duty_start_time": 61200,
  "fan_heat_cool_speed": "auto",
  "filter_changed_date": 1734220800,
  "gear_threshold_high": 0,
  "hvac_staging_ignore": false,
  "is_furnace_shutdown": false,
  "leaf_schedule_delta": 1.10999,
  "leaf_threshold_cool": 0,
  "leaf_threshold_heat": 21.4306,
  "oob_where_completed": true,
  "oob_wires_completed": true,
  "alt_heat_x2_delivery": "forced-air",
  "away_temperature_low": 18.52637,
  "dual_fuel_breakpoint": -1,
  "heat_link_connection": 0,
  "pin_star_description": "none",
  "away_temperature_high": 24.65883,
  "backplate_bsl_version": "3.1",
  "backplate_temperature": 21.14,
  "current_schedule_mode": "HEAT",
  "eco_onboarding_needed": true,
  "fan_cooling_readiness": "ready",
  "filter_reminder_level": 2,
  "has_hot_water_control": false,
  "hot_water_away_active": false,
  "oob_startup_completed": true,
  "oob_summary_completed": true,
  "pin_w2aux_description": "none",
  "preconditioning_ready": true,
  "backplate_mono_version": "4.2.8",
  "hot_water_away_enabled": true,
  "preconditioning_active": false,
  "backplate_serial_number": "02BA03AC331401HZ",
  "compressor_lockout_leaf": -17.79999,
  "filter_changed_set_date": 1564948047,
  "filter_reminder_enabled": true,
  "heat_pump_aux_threshold": 10,
  "heatpump_setback_active": false,
  "hot_water_boiling_state": true,
  "oob_interview_completed": true,
  "preconditioning_enabled": false,
  "radiant_control_enabled": false,
  "schedule_learning_reset": false,
  "should_wake_on_approach": true,
  "smoke_shutoff_supported": true,
  "target_humidity_enabled": false,
  "time_to_target_training": "ready",
  "heat_pump_comp_threshold": -31.5,
  "filter_replacement_needed": true,
  "has_hot_water_temperature": false,
  "lower_safety_temp_enabled": true,
  "sunlight_correction_ready": true,
  "temperature_lock_low_temp": 20,
  "temperature_lock_pin_hash": "",
  "upper_safety_temp_enabled": true,
  "hvac_safety_shutoff_active": false,
  "sunlight_correction_active": false,
  "temperature_lock_high_temp": 22.222,
  "safety_temp_activating_hvac": false,
  "sunlight_correction_enabled": true,
  "away_temperature_low_enabled": true,
  "away_temperature_high_enabled": true,
  "away_temperature_low_adjusted": 18.52637,
  "dual_fuel_breakpoint_override": "none",
  "last_software_update_utc_secs": 1569418914,
  "away_temperature_high_adjusted": 24.65883,
  "heat_pump_aux_threshold_enabled": true,
  "filter_replacement_threshold_sec": 1800000,
  "heat_pump_comp_threshold_enabled": false,
  "humidity_control_lockout_enabled": false,
  "hvac_smoke_safety_shutoff_active": false,
  "dehumidifier_orientation_selected": "unknown",
  "humidity_control_lockout_end_time": 0,
  "humidity_control_lockout_start_time": 0,
  "max_nighttime_preconditioning_seconds": 3600
}
```

### Field Initialization Notes

Some fields may be empty or "unknown" during device bootstrap before full state is reported:

| Field | Initial Value | Populated Value |
|-------|---------------|-----------------|
| `backplate_model` | `"unknown"` | `"Backplate-2.8"` |
| `backplate_serial_number` | `""` | `"02BA03AC331401HZ"` |
| `backplate_bsl_version` | `""` | `"3.1"` |
| `backplate_bsl_info` | `""` | `"BSL"` |
| `backplate_mono_version` | `""` | `"4.2.8"` |
| `backplate_mono_info` | `""` | `"TFE (BP_D2) 4.2.8..."` |
| `wiring_error` | `"Unknown"` | `""` |

---

## shared.{serial}

Current thermostat state and target temperatures.

```json
{
  "name": "",
  "can_cool": true,
  "can_heat": true,
  "auto_away": 0,
  "touched_by": {},
  "hvac_ac_state": false,
  "hvac_fan_state": false,
  "hvac_heater_state": false,
  "auto_away_learning": "ready",
  "hvac_cool_x2_state": false,
  "hvac_cool_x3_state": false,
  "hvac_heat_x2_state": false,
  "hvac_heat_x3_state": false,
  "target_temperature": 21.11111111111111,
  "current_temperature": 21.14,
  "hvac_alt_heat_state": false,
  "hvac_emer_heat_state": false,
  "hvac_aux_heater_state": false,
  "target_change_pending": false,
  "hvac_alt_heat_x2_state": false,
  "target_temperature_low": 20,
  "target_temperature_high": 24,
  "target_temperature_type": "heat",
  "compressor_lockout_enabled": false,
  "compressor_lockout_timeout": 0
}
```

### Key Fields

| Field | Type | Description |
|-------|------|-------------|
| `target_temperature` | number | Current target temperature (Celsius) |
| `current_temperature` | number | Current ambient temperature (Celsius) |
| `target_temperature_type` | string | Mode: `heat`, `cool`, `range`, `off` |
| `target_temperature_low` | number | Low target for range mode |
| `target_temperature_high` | number | High target for range mode |
| `hvac_heater_state` | boolean | Is heater currently running |
| `hvac_ac_state` | boolean | Is AC currently running |
| `hvac_fan_state` | boolean | Is fan currently running |
| `can_heat` | boolean | Device has heating capability |
| `can_cool` | boolean | Device has cooling capability |
| `weather` | object | Weather data (see below) |

### Weather Object (shared.{serial}.weather)

The `weather` property contains current conditions, daily forecasts, and hourly forecasts.

```json
{
  "weather": {
    "current": {
      "icon": "partlycloudy",
      "temp_c": 10,
      "humidity": 71,
      "wind_kph": 11.265407999999999,
      "wind_dir": "NW",
      "sunrise": 1734440505,
      "sunset": 1734474305,
      "conditions": "Partly Cloudy"
    },
    "forecast": {
      "daily": [
        {
          "date": 1734415200,
          "icon": "mostlycloudy",
          "high_temperature": 13,
          "low_temperature": 6,
          "conditions": "Partly Cloudy"
        },
        {
          "date": 1734501600,
          "icon": "sunny",
          "high_temperature": 7,
          "low_temperature": 1,
          "conditions": "Sunny"
        },
        {
          "date": 1734588000,
          "icon": "sunny",
          "high_temperature": 8,
          "low_temperature": 1,
          "conditions": "Sunny"
        },
        {
          "date": 1734674400,
          "icon": "sunny",
          "high_temperature": 10,
          "low_temperature": 2,
          "conditions": "Sunny"
        }
      ],
      "hourly": [
        { "time": 1734476400, "icon": "cloudy", "temp_c": 10, "humidity": 70, "wind_kph": 11.265407999999999, "wind_dir": "N" },
        { "time": 1734480000, "icon": "cloudy", "temp_c": 10, "humidity": 71, "wind_kph": 9.656064, "wind_dir": "N" },
        { "time": 1734483600, "icon": "cloudy", "temp_c": 10, "humidity": 74, "wind_kph": 8.0467199999999995, "wind_dir": "N" },
        { "time": 1734487200, "icon": "cloudy", "temp_c": 10, "humidity": 76, "wind_kph": 8.0467199999999995, "wind_dir": "NNE" },
        { "time": 1734490800, "icon": "cloudy", "temp_c": 9, "humidity": 76, "wind_kph": 8.0467199999999995, "wind_dir": "NE" },
        { "time": 1734494400, "icon": "mostlycloudy", "temp_c": 9, "humidity": 78, "wind_kph": 8.0467199999999995, "wind_dir": "ENE" },
        { "time": 1734498000, "icon": "partlycloudy", "temp_c": 8, "humidity": 76, "wind_kph": 6.4373759999999996, "wind_dir": "ENE" },
        { "time": 1734501600, "icon": "clear", "temp_c": 7, "humidity": 74, "wind_kph": 6.4373759999999996, "wind_dir": "E" },
        { "time": 1734505200, "icon": "clear", "temp_c": 6, "humidity": 71, "wind_kph": 6.4373759999999996, "wind_dir": "SE" },
        { "time": 1734508800, "icon": "clear", "temp_c": 4, "humidity": 68, "wind_kph": 6.4373759999999996, "wind_dir": "SE" },
        { "time": 1734512400, "icon": "clear", "temp_c": 3, "humidity": 66, "wind_kph": 6.4373759999999996, "wind_dir": "SSE" },
        { "time": 1734516000, "icon": "clear", "temp_c": 2, "humidity": 66, "wind_kph": 6.4373759999999996, "wind_dir": "S" }
      ]
    },
    "location": {
      "city": "Belmar",
      "state": "NJ"
    },
    "now": 1734476471
  }
}
```

### Weather Sub-Objects

#### current

| Field | Type | Description |
|-------|------|-------------|
| `icon` | string | Icon name: `sunny`, `clear`, `partlycloudy`, `mostlycloudy`, `cloudy`, etc. |
| `temp_c` | number | Current temperature (Celsius) |
| `humidity` | number | Humidity percentage |
| `wind_kph` | number | Wind speed (km/h) |
| `wind_dir` | string | Wind direction: `N`, `NE`, `NW`, `S`, `SE`, `SW`, `E`, `W`, etc. |
| `sunrise` | number | Unix timestamp of sunrise |
| `sunset` | number | Unix timestamp of sunset |
| `conditions` | string | Human-readable conditions |

#### forecast.daily[]

| Field | Type | Description |
|-------|------|-------------|
| `date` | number | Unix timestamp (start of day) |
| `icon` | string | Icon name for day |
| `high_temperature` | number | High temp (Celsius) |
| `low_temperature` | number | Low temp (Celsius) |
| `conditions` | string | Human-readable conditions |

#### forecast.hourly[]

| Field | Type | Description |
|-------|------|-------------|
| `time` | number | Unix timestamp |
| `icon` | string | Icon name |
| `temp_c` | number | Temperature (Celsius) |
| `humidity` | number | Humidity percentage |
| `wind_kph` | number | Wind speed (km/h) |
| `wind_dir` | string | Wind direction |

#### location

| Field | Type | Description |
|-------|------|-------------|
| `city` | string | City name |
| `state` | string | State/region code |

---

## schedule.{serial}

Weekly heating/cooling schedule.

```json
{
  "ver": 2,
  "name": "Current Schedule",
  "schedule_mode": "HEAT",
  "days": {
    "0": {
      "0": {
        "temp": 22.52321,
        "time": 0,
        "type": "HEAT",
        "entry_type": "continuation",
        "touched_at": 1766165915,
        "touched_by": 1,
        "touched_tzo": -18000
      },
      "1": {
        "temp": 19.60654,
        "time": 3600,
        "type": "HEAT",
        "entry_type": "setpoint",
        "touched_at": 1762102309,
        "touched_by": 2,
        "touched_tzo": -18000
      }
    }
  }
}
```

### Schedule Structure

| Field | Type | Description |
|-------|------|-------------|
| `ver` | number | Schedule version |
| `name` | string | Schedule name |
| `schedule_mode` | string | `HEAT`, `COOL`, or `RANGE` |
| `days` | object | Day 0-6 (Sunday-Saturday) |

### Setpoint Entry

| Field | Type | Description |
|-------|------|-------------|
| `temp` | number | Target temperature (Celsius) |
| `time` | number | Seconds from midnight |
| `type` | string | `HEAT` or `COOL` |
| `entry_type` | string | `setpoint` or `continuation` |
| `touched_at` | number | Unix timestamp of last modification |
| `touched_by` | number | Who modified (1=device, 2=user, etc.) |
| `touched_tzo` | number | Timezone offset in seconds |

---

## structure.{id}

Home/structure information.

```json
{
  "away": false,
  "city": "",
  "name": "Home",
  "user": "user.62769316",
  "state": "",
  "devices": ["device.02AA01AC33140F61"],
  "time_zone": "America/Chicago",
  "house_type": "unknown",
  "touched_by": {
    "touched_by": 3,
    "touched_id": "",
    "touched_user_id": ""
  },
  "postal_code": "07719",
  "tou_enabled": true,
  "country_code": "US",
  "address_lines": [],
  "vacation_mode": false,
  "away_timestamp": 0,
  "manual_eco_all": false,
  "num_thermostats": "1",
  "renovation_date": "",
  "dr_reminder_enabled": true,
  "manual_eco_timestamp": 0,
  "demand_charge_enabled": true,
  "manual_away_timestamp": 0,
  "diamond_changed_location": true,
  "eta_preconditioning_active": false,
  "hvac_safety_shutoff_enabled": true,
  "hvac_smoke_safety_shutoff_enabled": false
}
```

### Key Fields

| Field | Type | Description |
|-------|------|-------------|
| `away` | boolean | Home/Away status |
| `name` | string | Structure name |
| `devices` | string[] | List of device object keys |
| `time_zone` | string | IANA timezone |
| `postal_code` | string | ZIP/postal code |
| `country_code` | string | Two-letter country code |

---

## user.{id}

User account information.

```json
{
  "away": false,
  "name": "Guest",
  "email": "guest@gmail.com",
  "user_id": "62769316",
  "short_name": "",
  "structures": ["structure.ddcadb6a-86d3-47c6-b177-847e3a891f87"],
  "away_setter": 0,
  "away_timestamp": 0,
  "jasper_version": "5.84d3",
  "obsidian_version": "5.58rc3",
  "structure_memberships": [
    {
      "roles": ["owner"],
      "structure": "structure.ddcadb6a-86d3-47c6-b177-847e3a891f87"
    }
  ]
}
```

---

## link.{serial}

Links a device to its structure.

```json
{
  "structure": "structure.ddcadb6a-86d3-47c6-b177-847e3a891f87"
}
```

---

## device_alert_dialog.{serial}

Alert dialog state for the device display.

```json
{
  "dialog_id": "confirm-pairing",
  "dialog_data": "Connected"
}
```

---

## API Endpoints

### GET /entry

**Service discovery endpoint** - returns URLs for all Nest API services. This is typically the first endpoint a device contacts to bootstrap its API configuration.

**Request:** No authentication required.

**Response:**
```json
{
  "czfe_url": "https://frontdoor.nest.com/nest/transport",
  "transport_url": "https://frontdoor.nest.com/nest/transport",
  "direct_transport_url": "https://frontdoor.nest.com/nest/transport",
  "passphrase_url": "https://frontdoor.nest.com/nest/passphrase",
  "ping_url": "https://frontdoor.nest.com/nest/transport",
  "pro_info_url": "https://frontdoor.nest.com/nest/pro_info",
  "weather_url": "https://frontdoor.nest.com/nest/weather/v1?query=",
  "upload_url": "",
  "software_update_url": "",
  "server_version": "1.0.0",
  "tier_name": "local"
}
```

| Response Field | Type | Description |
|----------------|------|-------------|
| `czfe_url` | string | Main transport URL (CZFE = Client Zone Front End) |
| `transport_url` | string | Transport API base URL |
| `direct_transport_url` | string | Direct transport URL (may differ for NAT traversal) |
| `passphrase_url` | string | Endpoint to obtain device pairing passphrase |
| `ping_url` | string | Health check endpoint |
| `pro_info_url` | string | Nest Pro installer information endpoint |
| `weather_url` | string | Weather API endpoint (append query string) |
| `upload_url` | string | Firmware/data upload endpoint (empty if disabled) |
| `software_update_url` | string | Software update endpoint (empty if disabled) |
| `server_version` | string | Server version identifier |
| `tier_name` | string | Deployment tier (`local`, `prod`, etc.) |

---

### GET /passphrase

Returns a time-limited pairing passphrase for the device. This passphrase is displayed on the thermostat screen during setup and allows users to confirm they have physical access to the device.

**Authentication:** Required - Basic auth with device credentials.

**Request Headers:**
```
Authorization: Basic {base64(nest.{serial}:password)}
```

**Response (200):**
```json
{
  "value": "081CLYR",
  "expires": 1766002838775
}
```

| Response Field | Type | Description |
|----------------|------|-------------|
| `value` | string | 7-character alphanumeric passphrase |
| `expires` | number | Expiration timestamp (ms epoch) |

**Error Response (401):**
```json
{
  "error": "Unauthorized: Device serial required"
}
```

> **Note:** The passphrase is displayed on the thermostat screen during pairing/setup. It expires after a short period (typically a few minutes).

---

### POST /upload

Device telemetry/data upload endpoint.

**Request:**
- Content-Type: `text/plain; charset=utf-8`
- Body: Telemetry data (format TBD)

**Response (200):**
```json
{
  "status": "ok"
}
```

> **Note:** Request body content not captured in logs - likely firmware logs or usage telemetry.

---

### GET /ping

Health check endpoint - verifies connection to Nest servers.

**Request:** No parameters required.

**Response:**
```json
{
  "status": "ok",
  "timestamp": 1765999302255
}
```

| Response Field | Type | Description |
|----------------|------|-------------|
| `status` | string | Server status (`ok`) |
| `timestamp` | number | Server timestamp (ms epoch) |

---

### GET /pro_info

Returns Nest Pro installer information. Requires `entry_code` query parameter.

**Query Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `entry_code` | Yes | Installer entry code |

**Error Response (400):**
```json
{
  "error": "Missing entry code"
}
```

> **Note:** Full response schema not yet documented - requires valid entry code.

---

### GET /transport/v7/device/{object_key}

Returns the list of objects associated with a device, **without values** - only revisions and timestamps.

**Response:**
```json
{
  "objects": [
    {"object_revision": 17, "object_timestamp": 1766166234, "object_key": "device.02AA01AC33140F61"},
    {"object_revision": 3, "object_timestamp": 1766166075, "object_key": "schedule.02AA01AC33140F61"},
    {"object_revision": 1548, "object_timestamp": 1766166230, "object_key": "shared.02AA01AC33140F61"},
    {"object_revision": 17938, "object_timestamp": 1, "object_key": "structure.ddcadb6a-86d3-47c6-b177-847e3a891f87"},
    {"object_revision": -23671, "object_timestamp": 1766162985, "object_key": "user.62769316"},
    {"object_revision": 17938, "object_timestamp": 1, "object_key": "link.02AA01AC33140F61"},
    {"object_revision": 11303, "object_timestamp": 1, "object_key": "device_alert_dialog.02AA01AC33140F61"}
  ]
}
```

> **Note:** `object_revision` can be negative.

---

### POST /transport/v7/put

Device sends partial updates to one or more objects.

**Request:**
```json
{
  "session": "18b430119e9502AA01AC33140F61",
  "objects": [
    {
      "object_key": "shared.02AA01AC33140F61",
      "base_object_revision": 1545,
      "value": {
        "hvac_heater_state": true
      }
    },
    {
      "object_key": "device.02AA01AC33140F61",
      "base_object_revision": 14,
      "value": {
        "oob_temp_completed": true,
        "oob_test_completed": true,
        "maint_band_lower": 0.39000,
        "maint_band_upper": 0.39000,
        "current_humidity": 44,
        "battery_level": 3.82200
      }
    }
  ]
}
```

**Response:**
```json
{
  "objects": [
    {
      "object_revision": 1546,
      "object_timestamp": 1766166214,
      "object_key": "shared.02AA01AC33140F61",
      "value": { /* full current state */ }
    },
    {
      "object_revision": 15,
      "object_timestamp": 1766166214,
      "object_key": "device.02AA01AC33140F61",
      "value": { /* full current state */ }
    }
  ]
}
```

| Request Field | Type | Description |
|---------------|------|-------------|
| `session` | string | `{mac_address}{serial}` |
| `objects` | array | Objects to update |
| `objects[].object_key` | string | Object identifier |
| `objects[].base_object_revision` | number | Expected current revision (optimistic concurrency) |
| `objects[].value` | object | Partial update - only changed fields |

---

### POST /transport/v7/subscribe

Device subscribes to real-time updates. Returns chunked streaming response.

**Request:**
```json
{
  "chunked": true,
  "session": "18b430119e9502AA01AC33140F61",
  "objects": [
    {"object_key": "device.02AA01AC33140F61", "object_revision": 17, "object_timestamp": 1766166234},
    {"object_key": "shared.02AA01AC33140F61", "object_revision": 1548, "object_timestamp": 1766166230},
    {"object_key": "schedule.02AA01AC33140F61", "object_revision": 3, "object_timestamp": 1766166075},
    {"object_key": "device_alert_dialog.02AA01AC33140F61", "object_revision": 11303, "object_timestamp": 1},
    {"object_key": "structure.ddcadb6a-86d3-47c6-b177-847e3a891f87", "object_revision": 17938, "object_timestamp": 1},
    {"object_key": "link.02AA01AC33140F61", "object_revision": 17938, "object_timestamp": 1},
    {"object_key": "user.62769316", "object_revision": -23671, "object_timestamp": 1766162985}
  ]
}
```

| Request Field | Type | Description |
|---------------|------|-------------|
| `chunked` | boolean | Always `true` for streaming |
| `session` | string | `{mac_address}{serial}` |
| `objects` | array | Objects to subscribe with current revisions |
| `objects[].object_key` | string | Object identifier |
| `objects[].object_revision` | number | Last known revision |
| `objects[].object_timestamp` | number | Last known timestamp |

**Response:** Chunked streaming - sends object updates when state changes.

**Response Headers (Streaming):**

| Header | Example | Description |
|--------|---------|-------------|
| `X-Nl-Service-Timestamp` | `1766166211318` | Server timestamp (ms epoch) |
| `Transfer-Encoding` | `chunked` | Streaming enabled |
| `Content-Type` | `application/json; charset=UTF-8` | Response format |

---

### GET /weather/v1

Device requests weather data for location.

**Query Parameters:**

| Parameter | Example | Description |
|-----------|---------|-------------|
| `query` | `07719,US` | `{zip},{country_code}` |

**Response:**
```json
{
  "07719,US": {
    "current": {
      "temp_f": 51,
      "temp_c": 10.6,
      "condition": "Rain",
      "sunrise": 1766146440,
      "sunset": 1766179980,
      "humidity": 90,
      "gmt_offset": "-05.00",
      "wind_dir": "WSW",
      "wind_mph": 19,
      "icon": "rain"
    },
    "location": {
      "station_id": "unknown",
      "zip": "07719",
      "city": "Lake Como",
      "state": "NJ",
      "country": "US",
      "lat": "40.164221",
      "lon": "-74.088758",
      "short_name": "Lake Como,NJ",
      "timezone": "EST",
      "timezone_long": "America/New_York",
      "full_name": "Lake Como, NJ 07719, USA",
      "gmt_offset": "-05.00"
    },
    "forecast": {
      "daily": [
        {
          "temp_low_f": 28,
          "temp_low_c": -2.2,
          "temp_high_f": 58,
          "temp_high_c": 14.4,
          "humidity": 72,
          "condition": "Rain",
          "icon": "rain",
          "date": 1766120400
        }
      ],
      "hourly": [
        {
          "hour": 1,
          "temp_f": 53,
          "temp_c": 11.7,
          "humidity": 93,
          "time": 1766163600
        }
      ]
    }
  }
}
```

| Response Field | Type | Description |
|----------------|------|-------------|
| `{query}` | object | Weather data keyed by query string |
| `current.temp_f` | number | Current temperature (Fahrenheit) |
| `current.temp_c` | number | Current temperature (Celsius) |
| `current.condition` | string | Weather condition text |
| `current.sunrise` | number | Sunrise epoch timestamp |
| `current.sunset` | number | Sunset epoch timestamp |
| `current.humidity` | number | Humidity percentage |
| `current.wind_dir` | string | Wind direction (compass) |
| `current.wind_mph` | number | Wind speed (mph) |
| `current.icon` | string | Icon code |
| `location.city` | string | City name |
| `location.state` | string | State code |
| `location.timezone_long` | string | IANA timezone |
| `forecast.daily[]` | array | 6-day daily forecast |
| `forecast.hourly[]` | array | 48-hour hourly forecast |

> **Note:** Weather endpoint does NOT require authentication headers.

---

## Request Headers

From observed traffic:

| Header | Example | Description |
|--------|---------|-------------|
| `User-Agent` | `AddLightness/5.9.4-5 (Display-2.12; wireless_reg_domain=A2)` | Device firmware info |
| `Authorization` | `Basic {base64}` | `{prefix}.{serial}:{token}` |
| `X-NL-Protocol-Version` | `1` | Nest protocol version |
| `X-NL-Device-SWVersion` | `5.9.4-5` | Device software version |
| `Content-Type` | `application/json` | Request content type |

## Response Headers

| Header | Example | Description |
|--------|---------|-------------|
| `Content-Type` | `application/json; charset=UTF-8` | Response content type |
| `X-Nl-Service-Timestamp` | `1766167585160` | Server timestamp (ms) |
| `Transfer-Encoding` | `chunked` | Streaming response |

---

## Error Responses

Standard error response format:

```json
{
  "error": "Error message description"
}
```

| HTTP Status | Error Message | Description |
|-------------|---------------|-------------|
| 400 | `Missing entry code` | `/pro_info` called without `entry_code` param |
| 400 | `Missing query parameter` | `/weather/v1` called without `query` param |
| 401 | `Unauthorized: Device serial required` | Authentication header missing or invalid |
| 404 | `Not Found` | Unknown endpoint path |
| 502 | N/A | SSL/TLS connection failure to upstream |

---

## Changelog

| Date | Change |
|------|--------|
| 2025-12-19 | Added GET /entry service discovery endpoint |
| 2025-12-19 | Added GET /passphrase pairing code endpoint |
| 2025-12-19 | Added POST /upload telemetry endpoint |
| 2025-12-19 | Added 401 and 404 error codes to Error Responses |
| 2025-12-19 | Added GET /ping health check endpoint |
| 2025-12-19 | Added GET /pro_info endpoint (Nest Pro installer info) |
| 2025-12-19 | Added Error Responses section with common error codes |
| 2025-12-19 | Added device field initialization notes (bootstrap state) |
| 2025-12-19 | Added GET /weather/v1 endpoint (unauthenticated) |
| 2025-12-19 | Added streaming response headers to POST subscribe |
| 2025-12-19 | Added API endpoint schemas: GET device, POST put, POST subscribe |
| 2025-12-19 | Added `weather` object schema within `shared.{serial}` |
| 2025-12-19 | Initial documentation from proxy logs |
