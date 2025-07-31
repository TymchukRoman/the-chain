using Godot;
using System.Collections.Generic;

public partial class TowerEnemy : CharacterBody3D
{
    [Export] public float Speed = 2.5f;
    [Export] public int MaxHealth = 80;
    [Export] public int DamageToTower = 15;
    
    private int _currentHealth;
    private Vector3 _targetPosition;
    private bool _isDead = false;
    private Game _game;
    private NavigationAgent3D _navigationAgent;
    private float _stuckTimer = 0.0f;
    private Vector3 _lastPosition;
    private const float STUCK_THRESHOLD = 0.1f;
    private const float STUCK_TIME = 2.0f;
    
    // Tower targeting
    private Tower _targetTower = null;
    private float _attackRange = 1.5f;
    private float _attackTimer = 0.0f;
    private const float ATTACK_COOLDOWN = 1.0f; // Attack every 1 second
    
    // Health bar components
    private Node3D _healthBarContainer;
    private MeshInstance3D _healthBarBackground;
    private MeshInstance3D _healthBarFill;
    private Label3D _healthTextLabel;
    private bool _healthBarVisible = false;
    
    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Root");
        _currentHealth = MaxHealth;
        _lastPosition = GlobalPosition;
        
        // Ensure proper collision layer setup
        CollisionLayer = 2; // Enemy layer
        CollisionMask = 1; // Tower layer
        
        SetupNavigationAgent();
        SetupHealthBar();
        FindTargetTower();
        
        GD.Print("TowerEnemy spawned at: " + GlobalPosition + " with collision layer: " + CollisionLayer + ", collision mask: " + CollisionMask);
    }
    
    public void InitializeHealth()
    {
        _currentHealth = MaxHealth;
        UpdateHealthBar();
        GD.Print("TowerEnemy health initialized: " + _currentHealth + "/" + MaxHealth);
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
        fillMaterial.AlbedoColor = new Color(0.2f, 0.6f, 1.0f, 1.0f); // Blue color for tower enemies
        fillMaterial.EmissionEnabled = true;
        fillMaterial.Emission = new Color(0.2f, 0.6f, 1.0f, 1.0f);
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
        
        // Position health bar higher above enemy
        _healthBarContainer.Position = new Vector3(0, 1.5f, 0);
        
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
                fillMaterial.AlbedoColor = new Color(0.2f, 0.6f, 1.0f, 1.0f); // Blue
                fillMaterial.Emission = new Color(0.2f, 0.6f, 1.0f, 1.0f);
            }
            else if (healthPercentage > 0.3f)
            {
                fillMaterial.AlbedoColor = new Color(0.6f, 0.8f, 1.0f, 1.0f); // Light blue
                fillMaterial.Emission = new Color(0.6f, 0.8f, 1.0f, 1.0f);
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
    
    private void SetupNavigationAgent()
    {
        _navigationAgent = new NavigationAgent3D();
        AddChild(_navigationAgent);
        
        _navigationAgent.MaxSpeed = Speed;
        _navigationAgent.PathMaxDistance = 1.0f;
        _navigationAgent.PathDesiredDistance = 0.5f;
        
        GD.Print("TowerEnemy: Navigation agent set up");
    }
    
    private void FindTargetTower()
    {
        _targetTower = null;
        float closestDistance = float.MaxValue;
        
        // Get all towers in the scene
        var towers = GetTree().GetNodesInGroup("towers");
        GD.Print("TowerEnemy: Found " + towers.Count + " towers in group");
        
        foreach (var towerNode in towers)
        {
            if (towerNode is Tower tower)
            {
                                 if (tower.IsAlive())
                 {
                     float distance = GlobalPosition.DistanceTo(tower.GetTowerPosition());
                     GD.Print("TowerEnemy: Found tower at distance " + distance);
                     if (distance < closestDistance)
                     {
                         closestDistance = distance;
                         _targetTower = tower;
                     }
                 }
                else
                {
                    GD.Print("TowerEnemy: Found dead tower, skipping");
                }
            }
            else
            {
                GD.Print("TowerEnemy: Found non-tower node in towers group: " + towerNode.GetType());
            }
        }
        
                 if (_targetTower != null)
         {
             _targetPosition = _targetTower.GetTowerPosition();
             _navigationAgent.TargetPosition = _targetPosition;
             GD.Print("TowerEnemy targeting tower at: " + _targetPosition + " (distance: " + closestDistance + ")");
         }
        else
        {
            GD.Print("TowerEnemy: No towers found, targeting castle instead");
            _targetPosition = new Vector3(0.3f, 0, 6.6f); // Castle position
            _navigationAgent.TargetPosition = _targetPosition;
        }
    }
    
    public override void _Process(double delta)
    {
        if (_isDead) return;
        
        CheckIfStuck((float)delta);
        UpdateHealthBarRotation();
        
        // Check if target tower is still alive
        if (_targetTower != null && !_targetTower.IsAlive())
        {
            GD.Print("TowerEnemy: Target tower died, finding new target");
            FindTargetTower();
        }
        
                 // Attack if close to target
         if (_targetTower != null)
         {
             float distanceToTower = GlobalPosition.DistanceTo(_targetTower.GetTowerPosition());
             if (distanceToTower <= _attackRange)
             {
                 GD.Print("TowerEnemy: In attack range! Distance: " + distanceToTower);
                 AttackTower((float)delta);
                 return;
             }
             else
             {
                 GD.Print("TowerEnemy: Moving to tower, distance: " + distanceToTower);
             }
         }
        else
        {
            GD.Print("TowerEnemy: No target tower, moving to castle");
        }
        
        // Move towards target
        MoveAlongPath((float)delta);
    }
    
    private void CheckIfStuck(float delta)
    {
        float distanceMoved = GlobalPosition.DistanceTo(_lastPosition);
        
        if (distanceMoved < STUCK_THRESHOLD)
        {
            _stuckTimer += delta;
            if (_stuckTimer >= STUCK_TIME)
            {
                GD.Print("TowerEnemy stuck detected! Recalculating path...");
                FindTargetTower();
                _stuckTimer = 0.0f;
            }
        }
        else
        {
            _stuckTimer = 0.0f;
        }
        
        _lastPosition = GlobalPosition;
    }
    
    private void MoveAlongPath(float delta)
    {
        if (_navigationAgent.IsNavigationFinished())
        {
            if (_targetTower == null)
            {
                GD.Print("TowerEnemy reached castle!");
                _game.AddRP(-20);
                QueueFree();
            }
            return;
        }
        
        Vector3 nextPosition = _navigationAgent.GetNextPathPosition();
        Vector3 direction = (nextPosition - GlobalPosition).Normalized();
        
        Velocity = direction * Speed;
        MoveAndSlide();
    }
    
    private void AttackTower(float delta)
    {
        _attackTimer += delta;
        
        if (_attackTimer >= ATTACK_COOLDOWN)
        {
            _targetTower.TakeDamage(DamageToTower);
            _attackTimer = 0.0f;
            GD.Print("TowerEnemy attacked tower for " + DamageToTower + " damage!");
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        
        _currentHealth -= damage;
        if (_currentHealth < 0) _currentHealth = 0;
        
        GD.Print("TowerEnemy took " + damage + " damage! Health: " + _currentHealth + "/" + MaxHealth);
        UpdateHealthBar();
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (_isDead) return;
        
        _isDead = true;
        GD.Print("TowerEnemy died!");
        _game.AddRP(15); // More RP for killing tower enemies
        QueueFree();
    }
} 