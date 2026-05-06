# 🎮 TACTICAL BOOM - FPS Game Architecture

**Game Type:** Tactical FPS with Wave-based Defense  
**Platform:** Unity (C#)  
**Project Status:** Core Systems Implementation

---

## 📋 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Systems](#core-systems)
3. [Class Descriptions](#class-descriptions)
4. [Game Flow](#game-flow)
5. [Key Features](#key-features)
6. [File Structure](#file-structure)

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│              TACTICAL BOOM - FPS GAME                   │
├─────────────────────────────────────────────────────────┤
│  Core Systems          Gameplay Systems                 │
│  ├─ PlayerController   ├─ BombDefuseSystem             │
│  ├─ WeaponSystem       ├─ AIWaveManager                │
│  ├─ PlayerHealth       └─ EnemyController              │
│  └─ DamageHandler                                       │
│                                                         │
│  Management Systems    UI Systems                       │
│  ├─ MissionManager     ├─ HUDManager                    │
│  ├─ CurrencySystem     ├─ CrosshairUI                   │
│  └─ GameStateManager   └─ DefuseProgressBar            │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Core Systems

### 1. **DamageHandler.cs** - Advanced Hitbox System

**Purpose:** Calculate damage based on hit location  
**Key Features:**
- 8 Hitbox types (Head Top/Bottom, Center Mass, Arms, Legs, Torso)
- Critical zone detection (instant kill zones)
- Multiplier-based damage calculation
- Ragdoll physics integration

**Hitbox Damage Table:**

| Hitbox | Damage | Effect | Multiplier |
|--------|--------|--------|------------|
| HeadTop | 9999 (Instant Kill) | Ragdoll Force: 500 | 5.0x |
| HeadBottom | 100 (2 shots = death) | Ragdoll Force: 300 | 2.0x |
| CenterMass | 9999 (Instant Kill) | Ragdoll Force: 250 | 3.0x |
| LeftArm | 30 + Recoil Penalty | Ragdoll Force: 100 | 0.8x |
| RightArm | 30 + Recoil Penalty | Ragdoll Force: 100 | 0.8x |
| LeftLeg | 35 + Speed Reduction | Ragdoll Force: 150 | 0.8x |
| RightLeg | 35 + Speed Reduction | Ragdoll Force: 150 | 0.8x |
| Torso | 50 (Default) | Ragdoll Force: 120 | 1.0x |

**Main Method:**
```csharp
public DamageResult CalculateDamage(Vector3 hitPoint, Collider hitCollider, int baseBulletDamage = 50)
```

---

### 2. **PlayerHealth.cs** - Health & Injury Management

**Purpose:** Manage player health and injury states  
**Injury States:**

```
Healthy
├─ LeftArmInjured (Recoil +30%)
├─ RightArmInjured (Recoil +30%)
├─ LeftLegInjured (Speed -30%)
├─ RightLegInjured (Speed -30%)
├─ BothLegsInjured → CRAWL MODE
│   ├─ Speed: -75% (0.25x)
│   ├─ Defuse Time: +50% (1.5x)
│   └─ Cannot Sprint
└─ Dead
```

**Key Methods:**
- `ApplyArmDamage()` - Handle arm injuries
- `ApplyLegDamage()` - Handle leg injuries (Crawl when both injured)
- `GetRecoilMultiplier()` - Return recoil modifier
- `GetDefuseTimeMultiplier()` - Return defuse speed penalty
- `GetMoveSpeedMultiplier()` - Return movement speed modifier

---

### 3. **WeaponSystem.cs** - Weapon & Recoil Management

**Purpose:** Handle weapon firing, recoil, ammo, and attachments  
**Weapons Available:**

| Weapon | Damage | FireRate | Recoil (X/Y) | Accuracy | Price |
|--------|--------|----------|--------------|----------|-------|
| M4A1 | 50 | 0.1s (10 shots/sec) | 1.5/0.8 | 0.85 | Free |
| AK-47 | 60 | 0.15s (6.7 shots/sec) | 2.5/1.5 | 0.75 | $2500 |
| Glock 18 | 30 | 0.08s (12.5 shots/sec) | 0.8/0.4 | 0.60 | $500 |

**Recoil System:**
- Base Recoil per weapon
- Accumulating Recoil (pattern-based)
- Arm Injury Multiplier (up to +90% recoil)
- Attachment Recoil Reduction

**Attachment Types:**
- Scopes (Accuracy +0.1)
- Magazines (Ammo capacity)
- Muzzles (Recoil reduction -0.2 to -0.4)

---

### 4. **BombDefuseSystem.cs** - Bomb & Defuse Logic

**Purpose:** Manage bomb placement, defuse progress, and timer  
**Defuse Mechanics:**

```
DEFUSE FLOW:
1. Player approaches bomb (within 3m radius)
2. Press F to start defusing
3. Base defuse time: 5 seconds
4. If leg injured: defuse time = 5s × 1.5 = 7.5s
5. If defuse interrupted: reset progress
6. Success → Mission complete + Currency reward
7. Failure → Bomb explodes → Mission failed
```

**Bomb Properties:**
- Explosion Timer: 45 seconds
- Defuse Radius: 3 meters
- Explosion Damage: 150 HP
- Explosion Radius: 30 meters

**Events:**
- `OnDefuseProgressChanged` - Progress update (0-1)
- `OnDefuseComplete` - Defuse successful
- `OnDefuseInterrupted` - Defuse interrupted
- `OnBombTimerTick` - Countdown update
- `OnBombExploded` - Bomb exploded

---

### 5. **AIWaveManager.cs** - Wave-Based AI Spawning

**Purpose:** Manage enemy spawning waves during defuse  
**Wave Configuration:**

| Wave | Enemy Count | Spawn Delay | Difficulty |
|------|------------|------------|------------|
| 1 | 3 | 2.0s | Easy |
| 2 | 4 | 1.5s | Medium |
| 3 | 5 | 1.0s | Hard |

**Wave Logic:**
- Wave starts when defuse begins
- Enemies spawn at predefined locations
- All enemies eliminated → Next wave
- All waves cleared → Bonus event

---

### 6. **PlayerController.cs** - Input & Movement

**Purpose:** Handle player input and character movement  
**Controls:**
- `WASD` - Movement
- `Mouse` - Camera look
- `Left Click` - Fire weapon
- `R` - Reload
- `E` - Switch weapon / Interact
- `F` - Start/Stop defuse
- `Space` - Jump (not in crawl mode)
- `Shift` - Sprint (not in crawl mode)

**Movement Multipliers:**
- Normal: 5.0 m/s
- Sprint: 8.0 m/s
- Crawl: 1.0 m/s (when both legs injured)
- Leg Injured: ×0.7 (3.5 m/s normal)

---

### 7. **MissionManager.cs** - Mission State Management

**Purpose:** Manage mission states and win/loss conditions  
**Mission States:**

```
Preparation → DefusingPhase → MissionComplete
                ↓
          MissionFailed
```

**Win Conditions:**
- Defuse bomb successfully
- Player still alive
- All AI waves defeated

**Loss Conditions:**
- Bomb explodes (timeout)
- Player dies

**Rewards:**
- Base Reward: $2500
- Bonus: Wave completion bonuses

---

### 8. **CurrencySystem.cs** - Economy Management

**Purpose:** Manage in-game currency and purchases  
**Starting Currency:** $5000  
**Purchase Options:**

| Item | Price | Effect |
|------|-------|--------|
| AK-47 | $2500 | Alternative assault rifle |
| Glock 18 | $500 | Secondary weapon |
| Scope Attachment | $800 | +0.1 Accuracy |
| Suppressor | $1200 | -0.2 Recoil |
| Magazine Upgrade | $600 | +20 Ammo |

---

## 🎮 Game Flow

```
┌─────────────────────────────────────────────────────────┐
│                    GAME START                           │
└────────────────┬────────────────────────────────────────┘
                 ↓
        ┌─────────────────┐
        │  PREPARATION    │
        │  - Equip M4A1   │
        │  - Review map   │
        │  Press E to     │
        │  start mission  │
        └────────┬────────┘
                 ↓
        ┌─────────────────────────────────────┐
        │      DEFUSING PHASE STARTED         │
        │  - Bomb Timer: 45s                  │
        │  - Wave 1 spawns (3 enemies)        │
        └────────┬────────────────────────────┘
                 ↓
        ┌──────────────────────────────────────┐
        │   PLAYER APPROACHES BOMB            │
        │   - Must get within 3m              │
        │   - Press F to start defuse         │
        └────────┬─────────────────────────────┘
                 ↓
        ┌──────────────────────────────────────┐
        │   DEFUSE IN PROGRESS                │
        │   - Progress bar fills (5s base)    │
        │   - Avoid damage (interrupt)        │
        │   - Defend against waves            │
        └────────┬─────────────────────────────┘
                 ↓
          ┌─────┴─────┐
          ↓           ↓
   ┌─────────────┐  ┌──────────────┐
   │   SUCCESS   │  │   FAILURE    │
   │  DEFUSED!   │  │  - Exploded  │
   │  +$2500     │  │  - Player    │
   │  MISSION    │  │    died      │
   │  COMPLETE   │  │  MISSION     │
   └─────────────┘  │  FAILED      │
                    └──────────────┘
```

---

## ⚡ Key Features

### ✅ Advanced Hitbox System
- 8 distinct body parts with different damage profiles
- Instant-kill zones (Head/Center Mass)
- Critical shot detection
- Ragdoll physics on death

### ✅ Dynamic Injury System
- Arm injuries increase recoil (cumulative)
- Leg injuries reduce movement speed
- Both legs injured = Crawl mode (25% speed)
- Each state affects gameplay mechanics

### ✅ Realistic Recoil System
- Weapon-specific recoil patterns
- Arm injury multiplier
- Accumulating recoil over time
- Attachment recoil reduction

### ✅ Wave-Based AI Defense
- 3 waves with increasing difficulty
- Predictable spawn locations
- Intelligent enemy pathing to bomb
- Wave completion events

### ✅ Economy System
- Starting capital: $5000
- Mission rewards: $2500
- Weapon/attachment purchases
- Persistent currency between rounds

---

## 📁 File Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── DamageHandler.cs          (1/8)
│   │   ├── PlayerHealth.cs           (2/8)
│   │   ├── WeaponSystem.cs           (3/8)
│   │   └── PlayerController.cs       (6/8)
│   │
│   ├── Gameplay/
│   │   ├── BombDefuseSystem.cs       (4/8)
│   │   ├── AIWaveManager.cs          (5/8)
│   │   ├── EnemyController.cs        (TODO)
│   │   └── EnemyHealth.cs            (TODO)
│   │
│   ├── Management/
│   │   ├── MissionManager.cs         (7/8)
│   │   ├── CurrencySystem.cs         (8/8)
│   │   └── GameStateManager.cs       (TODO)
│   │
│   └── UI/
│       ├── HUDManager.cs             (TODO)
│       ├── CrosshairUI.cs            (TODO)
│       └── DefuseProgressBar.cs      (TODO)
│
├── Prefabs/
│   ├── Player.prefab
│   ├── AI_Terrorist.prefab
│   ├── M4A1.prefab
│   └── AK47.prefab
│
├── Resources/
│   ├── Effects/
│   │   ├── headshot_explosion
│   │   ├── chest_impact
│   │   ├── arm_hit
│   │   └── leg_hit
│   └── Audio/
│       ├── weapon_fire/
│       ├── footsteps/
│       └── bomb_beep/
│
└── Scenes/
    └── Mission_01.scene
```

---

## 🚀 Next Steps

### Phase 2: Enemy AI Implementation
- [ ] Implement `EnemyController.cs`
- [ ] Implement `EnemyHealth.cs`
- [ ] Pathfinding to bomb location
- [ ] Basic combat AI

### Phase 3: UI Implementation
- [ ] HUD Manager (health, ammo, timer)
- [ ] Crosshair system
- [ ] Defuse progress bar
- [ ] Damage indicators

### Phase 4: Polish & Balance
- [ ] VFX & Particle effects
- [ ] Audio system
- [ ] Game settings menu
- [ ] Difficulty levels

---

## 📊 Balance Sheet

**Health System:**
- Player HP: 200
- Critical Damage: Instant Kill
- Leg Injury Speed Reduction: 30% per leg
- Arm Injury Recoil Increase: 30% per arm

**Economy:**
- Starting Money: $5000
- Mission Reward: $2500
- Total Weapons: 3 (1 free, 2 purchasable)
- Total Attachments: 4

**Timing:**
- Base Defuse: 5 seconds
- Injured Defuse: 7.5 seconds
- Bomb Timer: 45 seconds
- Wave Gaps: 3 seconds

---

## 🔗 Additional Resources

- **Project Type:** FPS Tactical
- **Engine:** Unity 2021+
- **Language:** C#
- **Team Size:** Scalable
- **Estimated Completion:** Phase 3-4

---

**Last Updated:** 2026-05-06  
**Author:** Tactical Boom Dev Team  
**Status:** 🟢 Core Systems Complete
