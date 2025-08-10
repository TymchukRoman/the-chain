using Godot;
using System.Collections.Generic;

public static class GameConstants
{
    // Resource types
    public const string WOOD = "wood";
    public const string PEOPLE = "people";
    public const string STONE = "stone";
    
    // Tower costs by level
    public static readonly Dictionary<int, Dictionary<string, int>> TOWER_COSTS = new Dictionary<int, Dictionary<string, int>>
    {
        // Build cost (level 0 -> 1)
        { 0, new Dictionary<string, int> { { WOOD, 20 }, { PEOPLE, 5 } } },
        
        // Upgrade costs
        { 1, new Dictionary<string, int> { { WOOD, 30 }, { PEOPLE, 3 }, { STONE, 10 } } }, // Level 1 -> 2
        { 2, new Dictionary<string, int> { { WOOD, 40 }, { PEOPLE, 5 }, { STONE, 20 } } }  // Level 2 -> 3
    };
    
    // Tower stats by level
    public static readonly Dictionary<int, TowerStats> TOWER_STATS = new Dictionary<int, TowerStats>
    {
        { 1, new TowerStats { Damage = 25, FireRate = 1.0f, Range = 5.0f, MaxHealth = 100 } },
        { 2, new TowerStats { Damage = 40, FireRate = 1.5f, Range = 6.0f, MaxHealth = 150 } },
        { 3, new TowerStats { Damage = 60, FireRate = 2.0f, Range = 7.0f, MaxHealth = 200 } }
    };
    
    // Repair cost calculation
    public const int BASE_REPAIR_COST = 3; // Base repair cost in people
    
    // Demolish refund
    public const int DEMOLISH_REFUND_WOOD = 10;
    
    // Supply generation
    public const int SUPPLY_WOOD_PER_RANK = 5;
    public const int SUPPLY_PEOPLE_PER_RANK = 2;
    public const int SUPPLY_STONE_PER_RANK = 1;
}

// Tower stats structure
public struct TowerStats
{
    public int Damage;
    public float FireRate;
    public float Range;
    public int MaxHealth;
} 