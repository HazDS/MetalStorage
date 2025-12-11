# MetalStorage

A MelonLoader mod for Schedule 1 that adds metal variants of storage racks.

## Features

- Adds three new metal storage rack items:
  - Small Metal Storage Rack
  - Medium Metal Storage Rack
  - Large Metal Storage Rack
- Metal racks have an industrial look with the wood texture removed
- Available in the hardware store alongside regular storage racks
- Custom icons for each rack size
- Configurable extra storage slots per rack type, by default each rack has 2 extra slots
- Configurable prices

## Requirements

- Schedule 1 (Mono or IL2CPP version)
- MelonLoader: [Link](https://melonwiki.xyz/)
- S1API: [Github](https://github.com/ifBars/S1API/) - [Thunderstore](https://thunderstore.io/c/schedule-i/p/ifBars/S1API_Forked/) - [Nexus](https://www.nexusmods.com/schedule1/mods/1194)

## Installation

1. Install MelonLoader for Schedule 1
2. Install S1API (required dependency)
3. Copy `MetalStorage.dll` to your `Mods` folder (works with both Mono and IL2CPP)

## Building

```bash
# Build for Mono and IL2CPP
dotnet build MetalStorage.csproj -c CrossCompat

# Build for Mono
dotnet build MetalStorage.csproj -c Mono

# Build for IL2CPP
dotnet build MetalStorage.csproj -c Il2cpp
```

## How It Works

The mod clones the original storage rack definitions and registers them as new items. When placed, a Harmony patch intercepts the item creation and modifies the material to remove the wood texture, leaving a clean metal appearance.

## Configuration

After first run, a config file is created at `UserData/MelonPreferences.cfg`. You can adjust:

### Extra Slots

| Setting | Default | Base Slots | Max Extra | Description |
|---------|---------|------------|-----------|-------------|
| SmallExtraSlots | 2 | 4 | 16 | Extra slots for Small Metal Storage Rack |
| MediumExtraSlots | 2 | 6 | 14 | Extra slots for Medium Metal Storage Rack |
| LargeExtraSlots | 2 | 8 | 12 | Extra slots for Large Metal Storage Rack |

Example: Small rack has 4 base slots + 2 extra = 6 total slots. Note a maximum of 20 slots per rack are supported.

### Prices

| Setting | Default | Description |
|---------|---------|-------------|
| SmallRackPrice | 72 | Purchase price for Small Metal Storage Rack |
| MediumRackPrice | 104 | Purchase price for Medium Metal Storage Rack |
| LargeRackPrice | 144 | Purchase price for Large Metal Storage Rack |

## Credits

- [HazDS](https://github.com/HazDS) - Original mod author
- [ifBars](https://github.com/ifBars) - S1API cross-compatibility support
