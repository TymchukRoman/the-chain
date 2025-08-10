using Godot;
using System.Collections.Generic;

public partial class ResourceManager : Node
{
    private Dictionary<string, int> _resources = new()
    {
        { GameConstants.WOOD, 100 },
        { GameConstants.PEOPLE, 100 },
        { GameConstants.STONE, 50 }
    };

    private float _supplyTimer = 0.0f;
    private const float SUPPLY_INTERVAL = 20.0f; // Supply every 20 seconds

    private int _currentRank = 1;

    // Events
    public delegate void ResourceChangedHandler(string resourceType, int newAmount);
    public event ResourceChangedHandler OnResourceChanged;

    public delegate void SupplyGivenHandler(int woodAmount, int peopleAmount, int stoneAmount);
    public event SupplyGivenHandler OnSupplyGiven;

    public override void _Process(double delta)
    {
        UpdateSupplyTimer((float)delta);
    }

    private void UpdateSupplyTimer(float delta)
    {
        _supplyTimer += delta;
        if (_supplyTimer >= SUPPLY_INTERVAL)
        {
            GiveResourceSupply();
            _supplyTimer = 0.0f;
        }
    }

    private void GiveResourceSupply()
    {
        int woodToGive = GameConstants.SUPPLY_WOOD_PER_RANK * _currentRank;
        int peopleToGive = GameConstants.SUPPLY_PEOPLE_PER_RANK * _currentRank;
        int stoneToGive = GameConstants.SUPPLY_STONE_PER_RANK * _currentRank;
        
        _resources[GameConstants.WOOD] += woodToGive;
        _resources[GameConstants.PEOPLE] += peopleToGive;
        _resources[GameConstants.STONE] += stoneToGive;

        GD.Print($"Supply given: {woodToGive} wood, {peopleToGive} people, {stoneToGive} stone");
        
        // Emit events
        OnResourceChanged?.Invoke(GameConstants.WOOD, _resources[GameConstants.WOOD]);
        OnResourceChanged?.Invoke(GameConstants.PEOPLE, _resources[GameConstants.PEOPLE]);
        OnResourceChanged?.Invoke(GameConstants.STONE, _resources[GameConstants.STONE]);
        OnSupplyGiven?.Invoke(woodToGive, peopleToGive, stoneToGive);
    }

    public bool SpendResource(string type, int amount)
    {
        if (_resources.ContainsKey(type) && _resources[type] >= amount)
        {
            _resources[type] -= amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
            return true;
        }
        return false;
    }

    public bool HasResource(string type, int amount)
    {
        return _resources.ContainsKey(type) && _resources[type] >= amount;
    }

    public int GetResource(string type)
    {
        return _resources.ContainsKey(type) ? _resources[type] : 0;
    }

    public void AddResource(string type, int amount)
    {
        if (_resources.ContainsKey(type))
        {
            _resources[type] += amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
        }
    }

    public void SetRank(int rank)
    {
        _currentRank = rank;
    }

    public int GetRank()
    {
        return _currentRank;
    }

    public float GetTimeUntilSupply()
    {
        return SUPPLY_INTERVAL - _supplyTimer;
    }

    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(_resources);
    }
} 