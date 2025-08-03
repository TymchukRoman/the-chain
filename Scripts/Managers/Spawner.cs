using Godot;
using System.Collections.Generic;

public partial class Spawner : Node3D
{
    [Export] public PackedScene EnemyScene;
    [Export] public PackedScene BlueEnemyScene;
    [Export] public float InitialSpawnInterval = 1.5f;
    [Export] public int MaxEnemies = 15;
    [Export] public Vector3 SpawnAreaMin = new Vector3(-20, 0, -8);
    [Export] public Vector3 SpawnAreaMax = new Vector3(20, 0, -6);
    
    // Difficulty scaling
    [Export] public float DifficultyIncreaseInterval = 20.0f;
    [Export] public float HealthIncreasePerWave = 20.0f;
    [Export] public float SpeedIncreasePerWave = 0.2f;
    [Export] public float MaxSpeed = 5.0f;
    [Export] public float SpawnRateIncrease = 0.1f;
    [Export] public float MinSpawnInterval = 0.5f;
    
    private float _spawnTimer = 0.0f;
    private float _difficultyTimer = 0.0f;
    private List<RedEnemy> _activeEnemies = new List<RedEnemy>();
    private List<BlueEnemy> _activeTowerEnemies = new List<BlueEnemy>();
    private int _currentWave = 1;
    private float _currentSpawnInterval;
    private bool _isSpawning = false;

    // Events
    public delegate void WaveChangedHandler(int newWave);
    public event WaveChangedHandler OnWaveChanged;

    public delegate void EnemySpawnedHandler(RedEnemy enemy);
    public event EnemySpawnedHandler OnEnemySpawned;

    public delegate void TowerEnemySpawnedHandler(BlueEnemy enemy);
    public event TowerEnemySpawnedHandler OnTowerEnemySpawned;

    public override void _Ready()
    {
        _currentSpawnInterval = InitialSpawnInterval;
    }
    
    public override void _Process(double delta)
    {
        if (!_isSpawning) return;

        _spawnTimer += (float)delta;
        _difficultyTimer += (float)delta;
        
        // Increase difficulty over time
        if (_difficultyTimer >= DifficultyIncreaseInterval)
        {
            IncreaseDifficulty();
            _difficultyTimer = 0.0f;
        }
        
        // Spawn enemies continuously if under max limit
        if (_spawnTimer >= _currentSpawnInterval && (_activeEnemies.Count + _activeTowerEnemies.Count) < MaxEnemies)
        {
            SpawnEnemy();
            _spawnTimer = 0.0f;
        }
        
        // Clean up dead enemies
        _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));
        _activeTowerEnemies.RemoveAll(enemy => !IsInstanceValid(enemy));
    }
    
    private void IncreaseDifficulty()
    {
        _currentWave++;
        OnWaveChanged?.Invoke(_currentWave);
        
        // Calculate new stats
        int newHealth = 100 + (int)(HealthIncreasePerWave * (_currentWave - 1));
        float newSpeed = Mathf.Min(MaxSpeed, 2.0f + (SpeedIncreasePerWave * (_currentWave - 1)));
        
        // Increase spawn rate (decrease interval)
        _currentSpawnInterval = Mathf.Max(MinSpawnInterval, _currentSpawnInterval - SpawnRateIncrease);
        
        GD.Print($"Wave {_currentWave}: Health={newHealth}, Speed={newSpeed:F1}, SpawnRate={1.0f/_currentSpawnInterval:F1}/s");
    }
    
    private void SpawnEnemy()
    {
        // Randomly choose between regular enemy and tower enemy
        bool spawnTowerEnemy = GD.Randf() < 0.3f; // 30% chance for tower enemy
        
        if (spawnTowerEnemy && BlueEnemyScene != null)
        {
            SpawnTowerEnemy();
        }
        else if (EnemyScene != null)
        {
            SpawnRegularEnemy();
        }
    }
    
    private void SpawnRegularEnemy()
    {
        if (EnemyScene == null) return;
        
        // Random position in spawn area, but ensure it's within map boundaries
        float x = (float)GD.RandRange(SpawnAreaMin.X, SpawnAreaMax.X);
        float z = (float)GD.RandRange(SpawnAreaMin.Z, SpawnAreaMax.Z);
        
        // Clamp to map boundaries (assuming map is -25 to 25)
        x = Mathf.Clamp(x, -24, 24);
        z = Mathf.Clamp(z, -24, -6); // Keep in spawn area
        
        Vector3 spawnPosition = new Vector3(x, 0, z);
        
        // Check distance from castle (assuming castle is at origin)
        float distanceFromCastle = spawnPosition.Length();
        if (distanceFromCastle < 5.0f) // Too close to castle
        {
            return; // Don't spawn
        }
        
        var enemy = EnemyScene.Instantiate<RedEnemy>();
        GetParent().AddChild(enemy);
        enemy.GlobalPosition = spawnPosition;
        
        // Set enemy stats based on current wave
        int health = 100 + (int)(HealthIncreasePerWave * (_currentWave - 1));
        float speed = Mathf.Min(MaxSpeed, 2.0f + (SpeedIncreasePerWave * (_currentWave - 1)));
        
        enemy.Initialize(health, speed);
        _activeEnemies.Add(enemy);
        
        OnEnemySpawned?.Invoke(enemy);
    }
    
    private void SpawnTowerEnemy()
    {
        if (BlueEnemyScene == null) return;
        
        // Random position in spawn area
        float x = (float)GD.RandRange(SpawnAreaMin.X, SpawnAreaMax.X);
        float z = (float)GD.RandRange(SpawnAreaMin.Z, SpawnAreaMax.Z);
        
        // Clamp to map boundaries
        x = Mathf.Clamp(x, -24, 24);
        z = Mathf.Clamp(z, -24, -6);
        
        Vector3 spawnPosition = new Vector3(x, 0, z);
        
        // Check distance from castle
        float distanceFromCastle = spawnPosition.Length();
        if (distanceFromCastle < 5.0f)
        {
            return;
        }
        
        var enemy = BlueEnemyScene.Instantiate<BlueEnemy>();
        GetParent().AddChild(enemy);
        enemy.GlobalPosition = spawnPosition;
        
        // Set enemy stats
        int health = 100 + (int)(HealthIncreasePerWave * (_currentWave - 1));
        float speed = Mathf.Min(MaxSpeed, 2.0f + (SpeedIncreasePerWave * (_currentWave - 1)));
        
        enemy.Initialize(health, speed);
        _activeTowerEnemies.Add(enemy);
        
        OnTowerEnemySpawned?.Invoke(enemy);
    }
    
    public void StartSpawning()
    {
        _isSpawning = true;
        GD.Print("Enemy spawning started");
    }
    
    public void StopSpawning()
    {
        _isSpawning = false;
        GD.Print("Enemy spawning stopped");
    }
    
    public int GetCurrentWave()
    {
        return _currentWave;
    }

    public int GetActiveEnemyCount()
    {
        return _activeEnemies.Count + _activeTowerEnemies.Count;
    }
} 