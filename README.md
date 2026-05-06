# TACTICAL BOOM - FPS Game Core Systems

## 📋 Project Overview

**Tactical Boom** là một game FPS chiến thuật được thiết kế với hệ thống xử lý sát thương nâng cao, cơ chế gỡ bom, và quản lý sóng AI thông minh.

## 🎮 Core Features

### 1. **Advanced Hitbox Damage System** 🎯
- **HeadTop**: 1 viên = Tử vong ngay lập tức (Instant Kill)
- **HeadBottom**: 2 viên = Tử vong (100 damage mỗi viên)
- **CenterMass (Ngực)**: 1 viên = Tử vong (Instant Kill)
- **Arms (Tay)**: Tăng độ giật (Recoil multiplier +30% mỗi lần trúng)
- **Legs (Chân)**: Giảm tốc độ (-30%) hoặc CRAWL mode (cả 2 chân)
- **Torso**: Sát thương thường 50 damage

### 2. **Player Injury State Management** 🩹
```
Healthy
├─ LeftArmInjured (Recoil +30%)
├─ RightArmInjured (Recoil +30%)
├─ LeftLegInjured (Speed -30%)
├─ RightLegInjured (Speed -30%)
└─ BothLegsInjured → CRAWL MODE
   ├─ Speed: -75%
   ├─ Defuse Time: +50%
   └─ Can't Sprint/Jump
Dead
```

### 3. **Weapon System** 🔫
**M4A1 Carbine** (Default)
- Damage: 50
- Recoil: 1.5 (Y), 0.8 (X)
- Fire Rate: 0.1s (10 shots/sec)
- Accuracy: 0.85
- Ammo: 120

**AK-47**
- Damage: 60
- Recoil: 2.5 (Y), 1.5 (X) - **Higher recoil**
- Fire Rate: 0.15s (~6.7 shots/sec)
- Accuracy: 0.75
- Price: $2,500

**Glock 18 (G13)**
- Damage: 30
- Recoil: 0.8 (Y), 0.4 (X)
- Fire Rate: 0.08s (Fast)
- Accuracy: 0.6
- Price: $500

### 4. **Bomb Defuse System** 💣
- **Base Defuse Time**: 5 seconds
- **Bomb Timer**: 45 seconds (auto-explode)
- **Defuse Range**: 3 meters
- **Injury Multiplier**: x1.5 if both legs injured
- **Win Condition**: Complete defuse + Defeat all AI waves
- **Loss Condition**: Player death OR bomb explosion

### 5. **Wave-Based AI Defense** 🤖
**Wave 1**: 3 enemies
- Spawn Delay: 2s
- Equipped: AK-47/AKM

**Wave 2**: 4 enemies
- Spawn Delay: 1.5s
- Difficulty: Medium

**Wave 3**: 5 enemies
- Spawn Delay: 1s
- Difficulty: Heavy

### 6. **Currency & Shop System** 💰
- **Starting Currency**: $5,000
- **Mission Win Reward**: $2,500
- **Weapon Prices**: 
  - M4A1: Free (default)
  - AK-47: $2,500
  - Glock 18: $500

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── DamageHandler.cs          # Hitbox damage calculation
│   │   ├── PlayerHealth.cs           # Health & injury states
│   │   ├── WeaponSystem.cs           # Weapon & recoil system
│   │   └── PlayerController.cs       # Input & movement
│   ├── Gameplay/
│   │   ├── BombDefuseSystem.cs       # Bomb defuse mechanics
│   │   └── AIWaveManager.cs          # AI spawning & waves
│   └── Management/
│       ├── MissionManager.cs         # Mission state & objectives
│       └── CurrencySystem.cs         # Currency management
└── README.md
```

## 🔧 Key Scripts & Functions

### DamageHandler.cs
```csharp
public DamageResult CalculateDamage(Vector3 hitPoint, Collider hitCollider, int baseBulletDamage = 50)
```
**Flow**:
1. Xác định Hitbox từ bone name
2. Lấy DamageProfile từ bảng sát thương
3. Tính Final Damage = BaseDamage × Multiplier
4. Kiểm tra Instant Kill conditions
5. Return DamageResult struct

### PlayerHealth.cs
```csharp
public void ApplyArmDamage(DamageHandler.HitboxType armType)
public void ApplyLegDamage(DamageHandler.HitboxType legType, int damage)
```
**Arm Injury**: Tăng Recoil multiplier
**Leg Injury**: Giảm tốc độ di chuyển
**Both Legs**: Kích hoạt CRAWL mode (Speed -75%, Defuse +50%)

### BombDefuseSystem.cs
```csharp
public void StartDefuse()
public void InterruptDefuse()
public void CompleteDefuse()
public void ExplodeBomb()
```
**Defuse Flow**:
- Check Player in range (3m)
- Check Player alive
- Calculate defuse time with injury multiplier
- Update progress each frame
- Emit events to UI

### WeaponSystem.cs
```csharp
public void Fire(Vector3 shootDirection)
vector3 CalculateRecoil()
```
**Recoil Calculation**:
- Base Recoil from weapon stats
- Apply arm injury multiplier (+30% per hit)
- Add randomness (+/- 0.2-0.3)
- Accumulate over time

### AIWaveManager.cs
```csharp
public void StartWaveDefense()
void SpawnNextWave()
```
**Wave System**:
- Sequential wave spawning (1 → 2 → 3)
- Increasing difficulty
- 3-5 second delay between waves

## 🎮 Gameplay Flow

```
┌──────────────────────────────────────┐
│  START GAME                          │
│  - Player spawns with M4A1           │
│  - Receive $5,000 starting money     │
└─────────────┬──────────────────────┘
              │
              v
┌──────────────────────────────────────┐
│  PREPARATION PHASE                   │
│  - Buy weapons/attachments (optional)│
│  - Press E to start defuse           │
└─────────────┬──────────────────────┘
              │
              v
┌──────────────────────────────────────┐
│  DEFUSE PHASE STARTED                │
│  - Bomb Timer: 45s                   │
│  - Defuse Progress: 0-100%           │
│  - Wave 1 (3 AI) Spawns              │
└─────────────┬──────────────────────┘
              │
        ┌─────┴─────┐
        v           v
    ┌───────┐    ┌────────┐
    │ DEFUSE│    │COMBAT  │
    │  BOB  │    │  WAVES │
    └───────┘    └────────┘
        │           │
        ├─ Waves 1,2,3 Completed
        └─ Defuse 100%
        │
        v
┌──────────────────────────────────────┐
│  MISSION WIN ✓                       │
│  - Reward: $2,500                    │
│  - Total: $7,500                     │
└──────────────────────────────────────┘

          OR

┌──────────────────────────────────────┐
│  MISSION LOST ✗                      │
│  - Player Death (Headshot/Bomb)      │
│  - Bomb Exploded                     │
│  - Reset to Preparation Phase        │
└──────────────────────────────────────┘
```

## 🎯 Key Gameplay Mechanics

### Damage Calculation Example
```
Scenario 1: Headshot (Top)
- Base Damage: 50
- Multiplier: 5.0x
- Final Damage: 9999 (Instant Kill)
- Result: DEAD

Scenario 2: Chest Shot
- Base Damage: 50
- Multiplier: 3.0x
- Final Damage: 9999 (Instant Kill)
- Result: DEAD

Scenario 3: Leg Shot (Both)
- Damage: 35 each
- Effect: -30% speed per leg
- Both Hit: CRAWL MODE
- Crawl Speed: 1.25 m/s (-75%)
- Defuse Time: 7.5s (+50%)
```

### Recoil Pattern Example
```
M4A1 First Shot:
- Base Recoil X: 1.5
- Base Recoil Y: 0.8
- No Injury
- Final: 1.5x, 0.8x

After 2x Arm Hit:
- Multiplier: 1.6x (1.0 + 0.3 + 0.3)
- Recoil X: 1.5 × 1.6 = 2.4
- Recoil Y: 0.8 × 1.6 = 1.28
- Spray Pattern: Very hard to control
```

## 📊 State Transitions

```
Mission State Machine:
Preparation → DefusingPhase → MissionComplete (Win)
                           → MissionFailed (Loss)

Player Injury State Machine:
Healthy → [LeftArm/RightArm/LeftLeg/RightLeg] → BothLegsInjured → Dead

Wave State Machine:
Wave 1 → Wave 2 → Wave 3 → AllWavesCompleted
```

## 🎨 UI Integration Points

- **Health Bar**: Listen to `PlayerHealth.OnHealthChanged`
- **Injury Indicator**: Listen to `PlayerHealth.OnInjuryStateChanged`
- **Ammo Display**: Listen to `WeaponSystem.OnAmmoChanged`
- **Defuse Bar**: Listen to `BombDefuseSystem.OnDefuseProgressChanged`
- **Bomb Timer**: Listen to `BombDefuseSystem.OnBombTimerTick`
- **Wave Counter**: Listen to `AIWaveManager.OnWaveStarted`
- **Currency**: Listen to `CurrencySystem.OnCurrencyChanged`

## 🚀 Quick Start

1. **Add Player GameObject**:
   - Attach: PlayerHealth, WeaponSystem, PlayerController, DamageHandler
   - Add CharacterController component

2. **Add Game Manager**:
   - Attach: MissionManager, CurrencySystem, BombDefuseSystem, AIWaveManager

3. **Configure Bomb Site**:
   - Set bomb position in BombDefuseSystem
   - Set AI spawn points in AIWaveManager

4. **Create Enemy AI** (TODO):
   - Implement EnemyController.cs
   - Implement EnemyHealth.cs
   - Pathfinding to bomb position

5. **Create UI** (TODO):
   - HUD Manager
   - Crosshair
   - Defuse Progress Bar
   - Wave Counter

## 📝 TO-DO List

- [ ] Enemy AI Controller & Pathfinding
- [ ] Enemy Health & Damage handling
- [ ] Visual effects (bullet impact, explosions)
- [ ] Audio system (gunfire, explosions, UI sounds)
- [ ] UI/HUD (health bar, ammo, defuse bar, bomb timer)
- [ ] Animation system (shooting, reload, death, crawl)
- [ ] Weapon attachment UI & shop system
- [ ] Advanced recoil compensation
- [ ] Ragdoll physics integration
- [ ] Network multiplayer (future)

## 🔗 References

- **Event System**: Unity Action & Delegates
- **Physics**: Raycast, Collider hitboxes, Rigidbody ragdoll
- **State Management**: Enum-based state machines
- **Procedural Generation**: Wave spawning system

## 📄 License

This project is part of the "Tactical Boom" game development.

---

**Last Updated**: 2026-05-06  
**Author**: Game Dev Team  
**Status**: Core Systems Complete ✓
