using Godot;
using System.Collections.Generic;

public partial class Game : Node3D
{
    [Export] public NodePath UiPath;
    [Export] public PackedScene TowerScene;
    [Export] public PackedScene EnemyScene;
    [Export] public int TowerPeopleCost = 5;
    [Export] public int TowerWoodCost = 20;
    private GameUI _ui;
    private EnemySpawner _spawner;

    private int _rp = 100;
    private int _rank = 1;
    private bool _gameStarted = false;
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
    private float _lastFrameTime = 0.03f;
    public void BuildTowerOnCell(Cell cell)
    {
        if (cell.IsOccupied)
        {
            GD.Print("Cell already occupied");
            return;
        }

        // Check for both wood and people
        if (!HasResource("wood", TowerWoodCost) || !HasResource("people", TowerPeopleCost))
        {
            GD.Print("Not enough resources to build tower! Need 20 wood and " + TowerPeopleCost + " people");
            return;
        }

        var tower = TowerScene.Instantiate<Tower>();
        AddChild(tower);
        tower.GlobalPosition = cell.GlobalPosition;

        // Set the cell reference for cleanup when tower is destroyed
        tower.SetBuiltOnCell(cell);

        // Set the tower reference in the cell
        cell.SetBuiltTower(tower);

        // Spend both resources
        if (SpendResource("wood", TowerWoodCost) && SpendResource("people", TowerPeopleCost))
        {
            cell.MarkOccupied();
            GD.Print("Tower built successfully!");
        }

        // Start game after building first tower
        if (!_gameStarted)
        {
            StartGame();
        }
    }

    public override void _Ready()
    {
        _ui = GetNode<GameUI>(UiPath);
        _spawner = GetNode<EnemySpawner>("EnemySpawner");
        UpdateUI();
    }
    public override void _Process(double delta)
    {
        if (_gameStarted)
        {
            UpdateSupplyTimer((float)delta);
        }
    }
    private void UpdateSupplyTimer(float delta)
    {
        _supplyTimer += delta;
        if (_supplyTimer >= SUPPLY_INTERVAL)
        {
            GiveResourceSupply();
            _supplyTimer = 0.0f;
        }
        UpdateUI();
    }
    private void GiveResourceSupply()
    {
        int woodToGive = WOOD_PER_RANK * _rank;
        int peopleToGive = PEOPLE_PER_RANK * _rank;
        _resources["wood"] += woodToGive;
        _resources["people"] += peopleToGive;

        GD.Print($"Supply given: {woodToGive} wood, {peopleToGive} people");
    }
    private void StartGame()
    {
        _gameStarted = true;
        _spawner.StartSpawning();
    }

    public void AddRP(int amount)
    {
        _rp += amount;
        CheckRank();
        UpdateUI();
    }
    public void OnTowerDestroyed()
    {
        GD.Print("Tower destroyed!");
        
        // Close management panel if it's open
        var panel = GetNode<HexManagementPanel>("HexManagementPanel");
        if (panel != null && panel.Visible)
        {
            panel.Hide();
        }
    }
    public bool UpgradeTower(Tower tower)
    {
        if (tower == null)
        {
            GD.Print("Cannot upgrade: tower is null");
            return false;
        }
        
        if (!tower.CanUpgrade())
        {
            GD.Print("Cannot upgrade: tower at max level");
            return false;
        }
        
        int upgradeCost = tower.GetUpgradeCost();
        if (!HasResource("people", upgradeCost))
        {
            GD.Print("Cannot upgrade: insufficient people (need " + upgradeCost + ")");
            return false;
        }
        
        if (SpendResource("people", upgradeCost))
        {
            tower.TryUpgrade(this);
            GD.Print("Tower upgraded successfully!");
            return true;
        }
        
        return false;
    }
    public bool RepairTower(Tower tower)
    {
        if (tower == null)
        {
            GD.Print("Cannot repair: tower is null");
            return false;
        }
        
        if (!tower.CanRepair())
        {
            GD.Print("Cannot repair: tower at full health");
            return false;
        }
        
        int repairCost = tower.GetRepairCost();
        if (!HasResource("people", repairCost))
        {
            GD.Print("Cannot repair: insufficient people (need " + repairCost + ")");
            return false;
        }
        
        if (SpendResource("people", repairCost))
        {
            tower.Repair();
            GD.Print("Tower repaired successfully!");
            return true;
        }
        
        return false;
    }
    public void DemolishTower(Tower tower)
    {
        if (tower == null)
        {
            GD.Print("Cannot demolish: tower is null");
            return;
        }
        
        // Give refund for demolishing
        int refund = tower.DemolishRefund;
        _resources["wood"] += refund;
        
        GD.Print("Tower demolished! Refunded " + refund + " wood");
        
        // Demolish the tower (this will handle cell cleanup)
        tower.Demolish();
        
        // Update UI to show refund
        UpdateUI();
    }

    public bool SpendResource(string type, int amount)
    {
        if (_resources.ContainsKey(type) && _resources[type] >= amount)
        {
            _resources[type] -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public bool HasResource(string type, int amount)
    {
        return _resources.ContainsKey(type) && _resources[type] >= amount;
    }

    private void CheckRank()
    {
        // 10-rank system with proper progression and demotion
        int newRank = _rank;
        
        // Rank progression thresholds
        if (_rp >= 1000 && _rank < 10) newRank = 10;
        else if (_rp >= 800 && _rank < 9) newRank = 9;
        else if (_rp >= 600 && _rank < 8) newRank = 8;
        else if (_rp >= 450 && _rank < 7) newRank = 7;
        else if (_rp >= 350 && _rank < 6) newRank = 6;
        else if (_rp >= 250 && _rank < 5) newRank = 5;
        else if (_rp >= 200 && _rank < 4) newRank = 4;
        else if (_rp >= 150 && _rank < 3) newRank = 3;
        else if (_rp >= 100 && _rank < 2) newRank = 2;
        else if (_rp >= 50 && _rank < 1) newRank = 1;
        
        // Rank demotion thresholds (with some buffer to prevent constant bouncing)
        else if (_rp < 40 && _rank > 1) newRank = 1;
        else if (_rp < 80 && _rank > 2) newRank = 2;
        else if (_rp < 120 && _rank > 3) newRank = 3;
        else if (_rp < 170 && _rank > 4) newRank = 4;
        else if (_rp < 220 && _rank > 5) newRank = 5;
        else if (_rp < 320 && _rank > 6) newRank = 6;
        else if (_rp < 420 && _rank > 7) newRank = 7;
        else if (_rp < 520 && _rank > 8) newRank = 8;
        else if (_rp < 750 && _rank > 9) newRank = 9;
        else if (_rp < 950 && _rank > 10) newRank = 10;

        if (newRank != _rank)
        {
            if (newRank < _rank)
            {
                _rp = Mathf.Max(0, _rp - 20);
            }
            _rank = newRank;
        }
    }

    private void UpdateUI()
    {
        int currentWave = _spawner != null ? _spawner.GetCurrentWave() : 1;

        float timeUntilSupply = SUPPLY_INTERVAL - _supplyTimer;
        timeUntilSupply = Mathf.Max(0.0f, timeUntilSupply);

        _ui.UpdateUI(_rp, _rank, _resources, currentWave, timeUntilSupply);
    }
}
