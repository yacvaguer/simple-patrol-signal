# Simple Patrol Signal Plugin for Rust

**Simple Patrol Signal** is a plugin for Rust that allows server admins to add a dynamic patrol helicopter event to their game. Players can summon a patrol helicopter to their location using a specially designed signal. The helicopter will arrive, patrol the area, and then depart after a configurable duration. This adds an element of excitement and danger, especially during combat situations.

![image](https://codefling.com/uploads/monthly_2024_09/patrol.jpg.fb6998687a2ee59ef1da7b4d3ccce17f.jpg)

## Compatible Plugins 

- [No Escape](https://umod.org/plugins/no-escape)

## Features

- **Summon a Patrol Helicopter**: Players can use a special signal item to call a patrol helicopter to their location.
- **Configurable Helicopter Behavior**: You can configure the helicopter's health, patrol duration, number of loot crates it drops, and more.
- **Loot Drops**: Customize the loot dropped by the helicopter, including the types of crates and their drop chances.
- **Fully Configurable**: All settings, from the helicopter's health to the duration of its patrol, can be adjusted in the configuration file.

## Commands

### Chat Admin Command

- `/helisignal`: Admins can use this command to give themselves or a player a Patrol Heli Signal.
- `/helisignal reset {partial username}`: Remove the Cooldown for the player in order to use again the Heli Signal
- `/helisignal despawn`: Remove the current Attack Helicopter called by this plugin

### Console Command

- `helisignal`: Admins can use this command to give themselves or a player a Patrol Heli Signal from the server console.


## Permissions 

- `simplepatrolsignal.vip`: Players with this permission will get a different cooldwown that you can set on the configuration
- `simplepatrolsignal.use`: Basic Permission for all players to actually use the Heli Signal
- `simplepatrolsignal.admin`: Admin permission to use the chat commands 

- 

## Configuration Example

```json
{
  "Version": {
    "Major": 1,
    "Minor": 1,
    "Patch": 0
  },
  "Supply Signal Settings": {
    "Skin ID": 3332447426,
    "Display Name": "Patrol Heli Signal",
    "Warmup Time Before Patrol Arrival (seconds)": 5.0,
    "Default Cooldown Time (seconds)": 3600.0,
    "VIP Cooldown Time (seconds)": 1800.0
  },
  "Patrol Helicopter Settings": {
    "Patrol Duration (seconds)": 1800.0,
    "Helicopter Health": 10000.0,
    "Main Rotor Health": 900.0,
    "Tail Rotor Health": 500.0,
    "Number of Crates to Spawn": 6,
    "Time Before Firing Rockets (seconds)": 0.25
  },
  "Loot Settings": {
    "Enable Loot Drops": true,
    "Loot Containers and Drop Chances": {
      "crate_normal": 5.0,
      "crate_normal_2": 5.0,
      "crate_elite": 10.0,
      "heli_crate": 15.0,
      "bradley_crate": 15.0
    }
  },
  "Block During Raid": true,
  "Block During No Escape": true
}
```

### Key Configuration Options

• Supply Signal Settings: Control the signal’s display name, skin ID, and warm-up time before the helicopter arrives.
• Patrol Helicopter Settings: Configure key aspects of the helicopter, including its health, patrol duration, and when it fires rockets.
• Loot Settings: Specify which loot containers can drop, along with their chances.

### Supply Signal Skins

Default Skin: https://steamcommunity.com/sharedfiles/filedetails/?id=3332447426

![images](https://steamuserimages-a.akamaihd.net/ugc/2411203511269108830/6D682EEE399220D3D4ACC2BC50D0B23430E06D42/?imw=637&imh=358&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=true)

### Ideas for Server Integration

Here are some ideas on how to integrate the Patrol Heli Signal into your Rust server:

1. Add to Loot Tables: Make the signal a rare loot item in supply drops, Raidable Bases, or as custom loot.
2. Part of /kits: Add the Patrol Heli Signal as part of your kit rewards.
3. SkillTree: Include the Patrol Heli Signal as a reward in a SkillTree progression system.
4. Marketplace: If you have an in-game marketplace or shop, the Patrol Heli Signal can be a premium purchase.
5. Vending Machines: Make it purchasable in vending machines to increase its accessibility while keeping it balanced.

## Contributing

We welcome contributions from the Rust community! Whether it’s fixing a bug, suggesting improvements, or adding new features, your help is appreciated.

### How to Contribute

1. Fork the repository: Start by creating your own copy of the project.
2. Create a new branch: Make a new branch for your feature or bug fix.
3. Make your changes: Implement your code changes and test them thoroughly.
4. Submit a pull request: Open a pull request, providing a clear explanation of what your changes do and why they’re necessary.

### Reporting Issues

If you encounter bugs or have feature suggestions, feel free to report them in the Issues section. Please provide as much detail as possible to help us understand and resolve the issue.

## License

This project is licensed under the MIT License. You are free to use, modify, and distribute this software, as long as the original copyright notice and this permission notice appear in all copies.

```
MIT License

Copyright (c) 2024 Yac Vaguer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
