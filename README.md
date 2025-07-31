# Tower Defense Game - MVP

A simple tower defense game built with Godot 4 and C#.

## How to Play


1. **Start the Game**: Open the project in Godot and run the Game scene
2. **Build Towers**: Click on hex cells to build towers (costs 20 wood each)
3. **Defend the Castle**: Towers will automatically shoot at enemies
4. **Manage Resources**: You start with limited resources
5. **Earn Rank Points**:
   * Kill enemies: +10 RP
   * Enemies reach castle: -20 RP
   * Reach 200 RP to rank up!

## Game Features

### ✅ Implemented

* Hex-based map with castle
* Tower building system
* Resource management (wood, ammo, food, people)
* Enemy spawning from north
* Tower shooting mechanics
* Enemy movement toward castle
* Rank system with RP tracking
* Top-down camera controls

### 🎮 Controls

* **WASD**: Move camera
* **Mouse**: Look around
* **Mouse Wheel**: Zoom in/out
* **Left Click**: Build towers on cells
* **Escape**: Open menu

### 🏗️ Building System

* Click on any hex cell to build a tower
* Towers cost 20 wood each
* Towers automatically target and shoot nearby enemies
* Each tower has a range of 5 units

### ⚔️ Combat System

* Enemies spawn every 2 seconds from the north
* Towers shoot yellow projectiles at enemies
* Enemies have 100 HP, towers deal 25 damage
* Enemies move at 2 units/second toward the castle

### 📊 Resources & Ranking

* **Wood**: Used for building towers
* **Ammo**: Available for future weapon upgrades
* **Food**: Available for future unit feeding
* **People**: Available for future unit recruitment
* **Rank Points**: Earned by killing enemies, lost when enemies reach castle

## Development Notes

This is an MVP (Minimum Viable Product) that demonstrates core tower defense mechanics. The game automatically starts when you build your first tower, and enemies will begin spawning from the northern edge of the map.

### Future Enhancements

* Multiple tower types
* Enemy waves with different types
* Power-ups and upgrades
* Sound effects and music
* Better visual effects
* More complex pathfinding
* Multiple levels


