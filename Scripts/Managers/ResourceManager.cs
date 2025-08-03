using Godot;
using System.Collections.Generic;

public partial class ResourceManager : Node
{
    private Dictionary<string, int> _resources = new()
    {
        { "wood", 100 },
        { "ammo", 50 },
        { "food", 30 },
        { "people", 100 }
    };

    private float _supplyTimer = 0.0f;
    private const float SUPPLY_INTERVAL = 20.0f; // Supply every 20 seconds
    private const int WOOD_PER_RANK = 50; // Base wood per rank
    private const int PEOPLE_PER_RANK = 10; // Base people per rank

    private int _currentRank = 1;

    // Events
    public delegate void ResourceChangedHandler(string resourceType, int newAmount);
    public event ResourceChangedHandler OnResourceChanged;

    public delegate void SupplyGivenHandler(int woodAmount, int peopleAmount);
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
        int woodToGive = WOOD_PER_RANK * _currentRank;
        int peopleToGive = PEOPLE_PER_RANK * _currentRank;
        
        _resources["wood"] += woodToGive;
        _resources["people"] += peopleToGive;

        GD.Print($"Supply given: {woodToGive} wood, {peopleToGive} people");
        
        // Emit events
        OnResourceChanged?.Invoke("wood", _resources["wood"]);
        OnResourceChanged?.Invoke("people", _resources["people"]);
        OnSupplyGiven?.Invoke(woodToGive, peopleToGive);
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