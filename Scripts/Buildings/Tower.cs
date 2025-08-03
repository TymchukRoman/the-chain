using Godot;
using System;
using System.Collections.Generic;

public partial class Tower : Building
{
    [Export] public float Range = 5.0f;
    [Export] public float FireRate = 1.0f; // Shots per second
    [Export] public int Damage = 25;
    [Export] public PackedScene ProjectileScene;
    
    // Upgrade system
    [Export] public int UpgradeCost = 20; // Base cost for first upgrade
    [Export] public int MaxLevel = 3;
    [Export] public float DamageIncreasePerLevel = 15; // +15 damage per level
    [Export] public float FireRateIncreasePerLevel = 0.5f; // +0.3 fire rate per level
    [Export] public float RangeIncreasePerLevel = 1.0f; // +1.0 range per level
    
    // Cell reference for cleanup
    private Game _game;
    
    private float _fireTimer = 0.0f;
    private Node3D _currentTarget = null; // Can be Enemy or BlueEnemy
    private List<Node3D> _enemiesInRange = new List<Node3D>();
    private Area3D _detectionArea;
    
    // Level system
    private int _currentLevel = 1;
    private int _upgradeCost = 20; // Current upgrade cost
    
    // Visual level indicator
    private Node3D _levelIndicator;
    private MeshInstance3D _levelMesh;
    

    
    public override void _Ready()
    {
        base._Ready();
        
        // Add tower to group for pathfinding
        AddToGroup("towers");

        // Get the existing Area3D from the scene
        _detectionArea = GetNode<Area3D>("Area3D");

        // Set up the detection area
        var collisionShape = _detectionArea.GetNode<CollisionShape3D>("CollisionShape3D");
        var sphereShape = new SphereShape3D();
        sphereShape.Radius = Range;
        collisionShape.Shape = sphereShape;

        _detectionArea.CollisionLayer = 0;
        _detectionArea.CollisionMask = 2; // Enemy layer

        _detectionArea.BodyEntered += OnEnemyEntered;
        _detectionArea.BodyExited += OnEnemyExited;

        // Setup level indicator
        SetupLevelIndicator();
        
        // Initialize upgrade cost
        _upgradeCost = UpgradeCost;
        
        // Get reference to Game
        _game = GetNode<Game>("/root/Root");
    }
    
    public override void _Process(double delta)
    {
        if (_currentTarget == null || !IsInstanceValid(_currentTarget))
        {
            FindNewTarget();
        }
        
        if (_currentTarget != null)
        {
            _fireTimer += (float)delta;
            
            if (_fireTimer >= 1.0f / FireRate)
            {
                FireAtTarget();
                _fireTimer = 0.0f;
            }
        }
    }
    
    private void OnEnemyEntered(Node3D body)
    {
        if (body is Enemy || body is BlueEnemy)
        {
            _enemiesInRange.Add(body);
            if (_currentTarget == null)
            {
                FindNewTarget();
            }
        }
    }
    
    private void OnEnemyExited(Node3D body)
    {
        if (body is Enemy || body is BlueEnemy)
        {
            _enemiesInRange.Remove(body);
            if (_currentTarget == body)
            {
                _currentTarget = null;
                FindNewTarget();
            }
        }
    }
    
    private void FindNewTarget()
    {
        _currentTarget = null;
        
        // Find closest enemy in range
        float closestDistance = float.MaxValue;
        foreach (var enemy in _enemiesInRange)
        {
            if (IsInstanceValid(enemy))
            {
                float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    _currentTarget = enemy;
                }
            }
        }
        
        if (_currentTarget != null)
        {
        }
    }
    
    private void FireAtTarget()
    {
        if (_currentTarget == null || !IsInstanceValid(_currentTarget)) return;
        
        
        // Create projectile
        var projectile = new Projectile();
        AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition + Vector3.Up * 1.0f;
        projectile.Target = _currentTarget;
        projectile.Damage = Damage;
        projectile.Speed = 10.0f;
    }

    private void SetupLevelIndicator()
    {
        // Create level indicator container
        _levelIndicator = new Node3D();
        AddChild(_levelIndicator);
        
        // Create level mesh (ring around tower)
        var ringMesh = new CylinderMesh();
        ringMesh.TopRadius = 0.5f;
        ringMesh.BottomRadius = 0.5f;
        ringMesh.Height = 0.1f;
        ringMesh.RadialSegments = 16;
        
        var ringMaterial = new StandardMaterial3D();
        ringMaterial.AlbedoColor = GetLevelColor(_currentLevel);
        ringMaterial.EmissionEnabled = true;
        ringMaterial.Emission = GetLevelColor(_currentLevel);
        ringMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        ringMaterial.AlbedoColor = new Color(ringMaterial.AlbedoColor.R, ringMaterial.AlbedoColor.G, ringMaterial.AlbedoColor.B, 0.7f);
        ringMesh.Material = ringMaterial;
        
        _levelMesh = new MeshInstance3D();
        _levelMesh.Mesh = ringMesh;
        _levelMesh.Position = new Vector3(0, 0.1f, 0);
        _levelIndicator.AddChild(_levelMesh);
    }
    
    private Color GetLevelColor(int level)
    {
        switch (level)
        {
            case 1: return new Color(0.2f, 0.8f, 0.2f); // Green
            case 2: return new Color(1.0f, 0.8f, 0.2f); // Yellow
            case 3: return new Color(1.0f, 0.2f, 0.2f); // Red
            default: return new Color(0.5f, 0.5f, 0.5f); // Gray
        }
    }
    
    public bool CanUpgrade()
    {
        return _currentLevel < MaxLevel;
    }
    
    public int GetUpgradeCost()
    {
        return _upgradeCost;
    }
    
    public override int GetRepairCost()
    {
        // Calculate total tower cost including upgrades
        int totalTowerCost = 5; // Base tower cost (5 people)
        for (int i = 1; i < _currentLevel; i++)
        {
            totalTowerCost += 3 * i; // 3 * target level for each upgrade
        }
        
        // Calculate repair cost: total cost * (maxHealth - currentHealth) / 100 + 1
        int healthDifference = MaxHealth - _currentHealth;
        int repairCost = (totalTowerCost * healthDifference / 100) + 1;
        
        return repairCost;
    }
    
    protected override void RemoveFromGroups()
    {
        // Remove from towers group
        RemoveFromGroup("towers");
    }
    
    public bool TryUpgrade()
    {
        if (!CanUpgrade()) return false;
        
        // Check if player has enough resources
        var resourceManager = GetNode<ResourceManager>("/root/Root/ResourceManager");
        if (resourceManager == null || !resourceManager.SpendResource("people", _upgradeCost)) return false;
        
        // Upgrade the tower
        _currentLevel++;
        
        // Increase stats
        Damage += (int)DamageIncreasePerLevel;
        FireRate += FireRateIncreasePerLevel;
        Range += RangeIncreasePerLevel;
        
        // Update detection area
        var collisionShape = _detectionArea.GetNode<CollisionShape3D>("CollisionShape3D");
        var sphereShape = collisionShape.Shape as SphereShape3D;
        if (sphereShape != null)
        {
            sphereShape.Radius = Range;
        }
        
        // Update level indicator
        UpdateLevelIndicator();
        
        // Calculate next upgrade cost (3 * target level)
        _upgradeCost = 3 * (_currentLevel + 1);
        
        // Heal the tower when upgraded
        _currentHealth = MaxHealth;
        UpdateHealthBar();
        
        GD.Print($"Tower upgraded to level {_currentLevel}! Health restored to {_currentHealth}/{MaxHealth}");
        
        return true;
    }
    
    private void UpdateLevelIndicator()
    {
        if (_levelMesh == null) return;
        
        // Update ring color
        var ringMaterial = _levelMesh.GetActiveMaterial(0) as StandardMaterial3D;
        if (ringMaterial != null)
        {
            var newColor = GetLevelColor(_currentLevel);
            ringMaterial.AlbedoColor = newColor;
            ringMaterial.Emission = newColor;
        }
        
        // Update ring size
        var ringMesh = _levelMesh.Mesh as CylinderMesh;
        if (ringMesh != null)
        {
            ringMesh.TopRadius = 0.5f;
            ringMesh.BottomRadius = 0.5f;
        }
    }
    
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
        // Refresh management panel if it's open for this cell
        if (_builtOnCell != null)
        {
            var panel = GetNode<HexManagementPanel>("/root/Root/HexManagementPanel");
            if (panel != null)
            {
                panel.RefreshPanel();
            }
        }
        
        // Notify Game that tower was destroyed if this tower is destroyed
        if (_currentHealth <= 0 && _game != null)
        {
            // Remove from towers group when destroyed
            RemoveFromGroup("towers");
            _game.OnTowerDestroyed();
        }
    }
    
    
    public void Repair()
    {
        if (_currentHealth < MaxHealth)
        {
            _currentHealth = MaxHealth;
            UpdateHealthBar();
            GD.Print("Tower repaired to full health: " + _currentHealth + "/" + MaxHealth);
            
            // Refresh management panel if it's open for this cell
            if (_builtOnCell != null)
            {
                var panel = GetNode<HexManagementPanel>("/root/Root/HexManagementPanel");
                if (panel != null)
                {
                    panel.RefreshPanel();
                }
            }
        }
    }
    
    public void Demolish()
    {
        GD.Print("Tower demolished!");
        
        // Mark the cell as empty
        if (_builtOnCell != null)
        {
            _builtOnCell.RemoveTower();
            _builtOnCell.MarkEmpty();
        }
        
        // Notify Game that tower was demolished
        if (_game != null)
        {
            _game.OnTowerDestroyed();
        }
        
        QueueFree();
    }
    

}
