using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node3D
{
    [Export] public PackedScene EnemyScene;
    [Export] public float InitialSpawnInterval = 3.0f; // Starting spawn interval
    [Export] public int MaxEnemies = 15; // Increased max enemies
    [Export] public Vector3 SpawnAreaMin = new Vector3(-20, 0, -8);
    [Export] public Vector3 SpawnAreaMax = new Vector3(20, 0, -6);
    
    // Difficulty scaling
    [Export] public float DifficultyIncreaseInterval = 20.0f; // Increase difficulty every 30 seconds
    [Export] public float HealthIncreasePerWave = 20.0f; // +20 HP per wave
    [Export] public float SpeedIncreasePerWave = 0.2f; // +0.2 speed per wave
    [Export] public float MaxSpeed = 5.0f; // Maximum enemy speed
    [Export] public float SpawnRateIncrease = 0.1f; // Decrease spawn interval by 0.1s per wave
    [Export] public float MinSpawnInterval = 0.5f; // Minimum spawn interval
    
    private float _spawnTimer = 0.0f;
    private float _difficultyTimer = 0.0f;
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private Game _game;
    private int _currentWave = 1;
    private float _currentSpawnInterval;
    
    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Root");
        _currentSpawnInterval = InitialSpawnInterval;
        GD.Print("EnemySpawner initialized with spawn area: " + SpawnAreaMin + " to " + SpawnAreaMax);
        GD.Print("Starting Wave 1 - Enemies: 100 HP, 2.0 Speed, spawn every " + _currentSpawnInterval + "s");
    }
    
    public override void _Process(double delta)
    {
        _spawnTimer += (float)delta;
        _difficultyTimer += (float)delta;
        
        // Increase difficulty over time
        if (_difficultyTimer >= DifficultyIncreaseInterval)
        {
            IncreaseDifficulty();
            _difficultyTimer = 0.0f;
        }
        
        // Spawn enemies continuously if under max limit
        if (_spawnTimer >= _currentSpawnInterval && _activeEnemies.Count < MaxEnemies)
        {
            SpawnEnemy();
            _spawnTimer = 0.0f;
        }
        
        // Clean up dead enemies
        _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));
    }
    
    private void IncreaseDifficulty()
    {
        _currentWave++;
        
        // Calculate new stats
        int newHealth = 100 + (int)(HealthIncreasePerWave * (_currentWave - 1));
        float newSpeed = Mathf.Min(MaxSpeed, 2.0f + (SpeedIncreasePerWave * (_currentWave - 1)));
        
        // Increase spawn rate (decrease interval)
        _currentSpawnInterval = Mathf.Max(MinSpawnInterval, _currentSpawnInterval - SpawnRateIncrease);
        
        GD.Print("Wave " + _currentWave + " - Enemies: " + newHealth + " HP, " + newSpeed.ToString("F1") + " Speed, spawn every " + _currentSpawnInterval.ToString("F1") + "s");
    }
    
    private void SpawnEnemy()
    {
        if (EnemyScene == null) return;
        
        // Random position in spawn area, but ensure it's within map boundaries
        float x = (float)GD.RandRange(SpawnAreaMin.X, SpawnAreaMax.X);
        float z = (float)GD.RandRange(SpawnAreaMin.Z, SpawnAreaMax.Z);
        
        // Clamp to map boundaries (assuming map is -25 to 25)
        x = Mathf.Clamp(x, -24, 24);
        z = Mathf.Clamp(z, -24, 24);
        
        Vector3 spawnPosition = new Vector3(x, 0, z);
        
        // Check if spawn position is valid (not too close to castle)
        float distanceToCastle = spawnPosition.DistanceTo(new Vector3(0, 0, 6));
        if (distanceToCastle < 3.0f)
        {
            // Too close to castle, try a different position
            spawnPosition = new Vector3(
                (float)GD.RandRange(-20, 20),
                0,
                (float)GD.RandRange(-8, -4)
            );
        }
        
        var enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        enemy.GlobalPosition = spawnPosition;
        
        // Apply current wave stats
        int currentHealth = 100 + (int)(HealthIncreasePerWave * (_currentWave - 1));
        float currentSpeed = Mathf.Min(MaxSpeed, 2.0f + (SpeedIncreasePerWave * (_currentWave - 1)));
        
                            enemy.MaxHealth = currentHealth;
                    enemy.Speed = currentSpeed;
                    
                    // Initialize health after setting MaxHealth
                    enemy.InitializeHealth();

                    _activeEnemies.Add(enemy);

                    GD.Print("Enemy spawned at: " + spawnPosition + " (Wave " + _currentWave + " - " + currentHealth + " HP, " + currentSpeed.ToString("F1") + " Speed) [Active: " + _activeEnemies.Count + "/" + MaxEnemies + "]");
    }
    
    public void StopSpawning()
    {
        SetProcess(false);
    }
    
    public void StartSpawning()
    {
        SetProcess(true);
    }
    
    public int GetCurrentWave()
    {
        return _currentWave;
    }
} 