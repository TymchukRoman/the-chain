using Godot;
using System.Collections.Generic;

public partial class Game : Node3D
{
    [Export] public NodePath UiPath;
    [Export] public PackedScene TowerScene;
    [Export] public PackedScene EnemyScene;
    [Export] public int TowerCost = 20;
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
        { "people", 10 }
    };
    
    // Resource supply system
    private float _supplyTimer = 0.0f;
    private const float SUPPLY_INTERVAL = 20.0f; // Supply every 20 seconds
    private const int WOOD_PER_RANK = 50; // Base wood per rank
    private float _lastFrameTime = 0.0f;
    
    public void TryBuildTowerOn(Cell cell)
    {
        if (cell.IsOccupied)
        {
            GD.Print("Cell already occupied");
            return;
        }

        if (!HasResource("wood", TowerCost))
        {
            GD.Print("Not enough wood!");
            return;
        }

        var tower = TowerScene.Instantiate<Tower>();
        AddChild(tower);  // or parent to a TowerHolder node
        tower.GlobalPosition = cell.GlobalPosition;

        SpendResource("wood", TowerCost);
        cell.MarkOccupied();
        
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
        GD.Print("Game ready");
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
        // Use a more robust timer that's less affected by frame rate issues
        _supplyTimer += delta;
        
        // Clamp timer to prevent it from going over the interval
        if (_supplyTimer >= SUPPLY_INTERVAL)
        {
            GiveResourceSupply();
            _supplyTimer = 0.0f;
        }
        
        // Update UI more frequently for smooth countdown
        UpdateUI();
    }
    
    private void GiveResourceSupply()
    {
        int woodToGive = WOOD_PER_RANK * _rank;
        _resources["wood"] += woodToGive;
        
        GD.Print("Resource supply! +" + woodToGive + " wood (Rank " + _rank + ")");
    }
    
    private void StartGame()
    {
        _gameStarted = true;
        _spawner.StartSpawning();
        GD.Print("Game started! Enemies are spawning...");
    }

    public void AddRP(int amount)
    {
        _rp += amount;
        CheckRank();
        UpdateUI();
    }

    public void SpendResource(string type, int amount)
    {
        if (_resources.ContainsKey(type))
            _resources[type] = Mathf.Max(0, _resources[type] - amount);

        UpdateUI();
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
        
        // Handle rank changes
        if (newRank != _rank)
        {
            if (newRank > _rank)
            {
                GD.Print("RANK UP! Rank " + _rank + " → Rank " + newRank + " (RP: " + _rp + ")");
            }
            else
            {
                GD.Print("RANK DOWN! Rank " + _rank + " → Rank " + newRank + " (RP: " + _rp + ")");
                // Penalty for demotion - lose some RP
                _rp = Mathf.Max(0, _rp - 20);
            }
            _rank = newRank;
        }
    }

    private void UpdateUI()
    {
        int currentWave = _spawner != null ? _spawner.GetCurrentWave() : 1;
        
        // Calculate time until next supply more robustly
        float timeUntilSupply = SUPPLY_INTERVAL - _supplyTimer;
        timeUntilSupply = Mathf.Max(0.0f, timeUntilSupply); // Ensure it doesn't go negative
        
        _ui.UpdateUI(_rp, _rank, _resources, currentWave, timeUntilSupply);
    }
}
