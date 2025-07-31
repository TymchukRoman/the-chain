using Godot;
using System;
using System.Collections.Generic;

public partial class Tower : Node3D
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
    
    // Tower HP system
    [Export] public int MaxHealth = 100;
    private int _currentHealth;
    
    private float _fireTimer = 0.0f;
    private Node3D _currentTarget = null; // Can be Enemy or TowerEnemy
    private List<Node3D> _enemiesInRange = new List<Node3D>();
    private Area3D _detectionArea;
    
    // Level system
    private int _currentLevel = 1;
    private int _upgradeCost = 20; // Current upgrade cost
    
    // Visual level indicator
    private Node3D _levelIndicator;
    private MeshInstance3D _levelMesh;
    
    // Health bar components
    private Node3D _healthBarContainer;
    private MeshInstance3D _healthBarBackground;
    private MeshInstance3D _healthBarFill;
    private Label3D _healthTextLabel;
    private bool _healthBarVisible = false;
    
    public override void _Ready()
    {
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
        
        // Setup health bar
        SetupHealthBar();
        
        // Initialize upgrade cost
        _upgradeCost = UpgradeCost;
        
        // Initialize tower health
        _currentHealth = MaxHealth;

        GD.Print("Tower initialized with range: " + Range + ", level: " + _currentLevel + ", health: " + _currentHealth);
    }
    
    public override void _Process(double delta)
    {
        UpdateHealthBarRotation();
        
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
        if (body is Enemy || body is TowerEnemy)
        {
            GD.Print("Tower detected enemy entering range!");
            _enemiesInRange.Add(body);
            if (_currentTarget == null)
            {
                FindNewTarget();
            }
        }
    }
    
    private void OnEnemyExited(Node3D body)
    {
        if (body is Enemy || body is TowerEnemy)
        {
            GD.Print("Tower detected enemy leaving range!");
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
            GD.Print("Tower found new target! Distance: " + closestDistance);
        }
    }
    
    private void FireAtTarget()
    {
        if (_currentTarget == null || !IsInstanceValid(_currentTarget)) return;
        
        GD.Print("Tower FIRING at enemy! Damage: " + Damage);
        
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
    
    public bool TryUpgrade(Game game)
    {
        if (!CanUpgrade()) return false;
        
        // Check if player has enough resources
        if (!game.SpendResource("wood", _upgradeCost)) return false;
        
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
        
        // Calculate next upgrade cost
        _upgradeCost = UpgradeCost * _currentLevel+1; // 20, 40, 60
        
        GD.Print("Tower upgraded to level " + _currentLevel + "! Damage: " + Damage + ", FireRate: " + FireRate + ", Range: " + Range);
        
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
    
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth < 0) _currentHealth = 0;
        
        GD.Print("Tower took " + damage + " damage! Health: " + _currentHealth + "/" + MaxHealth);
        UpdateHealthBar();
        
        if (_currentHealth <= 0)
        {
            GD.Print("Tower destroyed!");
            QueueFree();
        }
    }
    
    public Vector3 GetTowerPosition()
    {
        return GlobalPosition;
    }
    
    public bool IsAlive()
    {
        return _currentHealth > 0;
    }
    
    private void SetupHealthBar()
    {
        // Create health bar container
        _healthBarContainer = new Node3D();
        AddChild(_healthBarContainer);
        
        // Create background bar
        var backgroundMesh = new BoxMesh();
        backgroundMesh.Size = new Vector3(2.0f, 0.2f, 0.1f);
        var backgroundMaterial = new StandardMaterial3D();
        backgroundMaterial.AlbedoColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        backgroundMaterial.EmissionEnabled = true;
        backgroundMaterial.Emission = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        backgroundMesh.Material = backgroundMaterial;
        
        _healthBarBackground = new MeshInstance3D();
        _healthBarBackground.Mesh = backgroundMesh;
        _healthBarContainer.AddChild(_healthBarBackground);
        
        // Create fill bar
        var fillMesh = new BoxMesh();
        fillMesh.Size = new Vector3(1.8f, 0.16f, 0.08f);
        var fillMaterial = new StandardMaterial3D();
        fillMaterial.AlbedoColor = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Green color for towers
        fillMaterial.EmissionEnabled = true;
        fillMaterial.Emission = new Color(0.2f, 1.0f, 0.2f, 1.0f);
        fillMesh.Material = fillMaterial;
        
        _healthBarFill = new MeshInstance3D();
        _healthBarFill.Mesh = fillMesh;
        _healthBarFill.Position = new Vector3(0, 0, 0.02f);
        _healthBarContainer.AddChild(_healthBarFill);
        
        // Create health text label
        _healthTextLabel = new Label3D();
        _healthTextLabel.Text = _currentHealth + "/" + MaxHealth;
        _healthTextLabel.FontSize = 24;
        _healthTextLabel.PixelSize = 0.02f;
        _healthTextLabel.Position = new Vector3(0, 0.3f, 0.03f);
        _healthTextLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _healthTextLabel.VerticalAlignment = VerticalAlignment.Center;
        _healthBarContainer.AddChild(_healthTextLabel);
        
        // Position health bar higher above tower
        _healthBarContainer.Position = new Vector3(0, 2.5f, 0);
        
        // Initially hide health bar
        _healthBarContainer.Visible = false;
    }
    
    private void UpdateHealthBar()
    {
        if (_healthBarContainer == null) return;
        
        float healthPercentage = (float)_currentHealth / MaxHealth;
        healthPercentage = Mathf.Clamp(healthPercentage, 0.0f, 1.0f);
        
        // Update fill bar width
        var fillMesh = _healthBarFill.Mesh as BoxMesh;
        if (fillMesh != null)
        {
            fillMesh.Size = new Vector3(1.8f * healthPercentage, 0.16f, 0.08f);
        }
        
        // Update fill bar color
        var fillMaterial = _healthBarFill.GetActiveMaterial(0) as StandardMaterial3D;
        if (fillMaterial != null)
        {
            if (healthPercentage > 0.6f)
            {
                fillMaterial.AlbedoColor = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Green
                fillMaterial.Emission = new Color(0.2f, 1.0f, 0.2f, 1.0f);
            }
            else if (healthPercentage > 0.3f)
            {
                fillMaterial.AlbedoColor = new Color(1.0f, 1.0f, 0.2f, 1.0f); // Yellow
                fillMaterial.Emission = new Color(1.0f, 1.0f, 0.2f, 1.0f);
            }
            else
            {
                fillMaterial.AlbedoColor = new Color(1.0f, 0.2f, 0.2f, 1.0f); // Red
                fillMaterial.Emission = new Color(1.0f, 0.2f, 0.2f, 1.0f);
            }
        }
        
        // Update health text
        if (_healthTextLabel != null)
        {
            _healthTextLabel.Text = _currentHealth + "/" + MaxHealth;
        }
        
        // Show health bar when damaged
        if (!_healthBarVisible && _currentHealth < MaxHealth)
        {
            _healthBarVisible = true;
            _healthBarContainer.Visible = true;
        }
    }
    
    private void UpdateHealthBarRotation()
    {
        if (_healthBarContainer == null) return;
        
        var camera = GetViewport().GetCamera3D();
        if (camera == null) return;
        
        _healthBarContainer.LookAt(camera.GlobalPosition);
        _healthBarContainer.RotateY(Mathf.Pi);
    }
}
