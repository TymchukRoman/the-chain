using Godot;
using System;

public abstract partial class Enemy : CharacterBody3D
{
    [Export] public float Speed = 2.0f;
    [Export] public int MaxHealth = 100;
    [Export] public int DamageToTarget = 10;
    
    protected int _currentHealth;
    protected NavigationAgent3D _navigationAgent;
    protected Node3D _currentTarget;
    protected float _stuckTimer = 0.0f;
    protected const float STUCK_THRESHOLD = 1.0f; // Recalculate path if stuck for 1 second
    protected Vector3 _lastPosition;
    
    // Health bar component
    protected HealthBar _healthBar;
    
    // Events
    public delegate void EnemyDiedHandler(Enemy enemy);
    public event EnemyDiedHandler OnEnemyDied;
    
    public delegate void EnemyReachedTargetHandler(Enemy enemy);
    public event EnemyReachedTargetHandler OnEnemyReachedTarget;

    public override void _Ready()
    {
        SetupNavigationAgent();
        SetupHealthBar();
        _lastPosition = GlobalPosition;
    }

    public virtual void Initialize(int health, float speed)
    {
        MaxHealth = health;
        _currentHealth = health;
        Speed = speed;
        UpdateHealthBar();
    }

    public override void _Process(double delta)
    {
        UpdateMovement((float)delta);
    }

    protected virtual void SetupNavigationAgent()
    {
        _navigationAgent = new NavigationAgent3D();
        AddChild(_navigationAgent);
        
        // Get navigation region from the map
        var map = GetNode<Map>("/root/Root/Map");
        if (map != null)
        {
            var navRegion = map.GetNavigationRegion();
            if (navRegion != null)
            {
                // In Godot 4.x, we don't need to set NavigationRegion directly
                // The NavigationAgent3D will automatically use the navigation mesh
            }
        }
    }

    protected virtual void SetupHealthBar()
    {
        // Create and setup the health bar
        _healthBar = new HealthBar();
        AddChild(_healthBar);
        _healthBar.Initialize(_currentHealth, MaxHealth);
    }

    protected virtual void UpdateHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(_currentHealth, MaxHealth);
        }
    }



    protected virtual void UpdateMovement(float delta)
    {
        if (_navigationAgent == null || _currentTarget == null) return;

        // Check if stuck
        float distanceMoved = GlobalPosition.DistanceTo(_lastPosition);
        if (distanceMoved < 0.05f) // More sensitive stuck detection
        {
            _stuckTimer += delta;
            if (_stuckTimer > STUCK_THRESHOLD)
            {
                RecalculatePath();
                _stuckTimer = 0.0f;
            }
        }
        else
        {
            _stuckTimer = 0.0f;
        }
        
        // Check if we're too close to any tower (potential obstacle)
        CheckForNearbyObstacles();
        
        _lastPosition = GlobalPosition;

        // Move towards target
        var nextPosition = _navigationAgent.GetNextPathPosition();
        var direction = (nextPosition - GlobalPosition).Normalized();
        
        Velocity = direction * Speed;
        MoveAndSlide();

        // Check if reached target
        // Check if target is still valid before accessing its position
        if (!IsTargetValid())
        {
            return;
        }
        
        var targetPosition = GetTargetPosition();
        if (!targetPosition.HasValue)
        {
            return;
        }
        
        if (GlobalPosition.DistanceTo(targetPosition.Value) < 1.0f)
        {
            OnReachedTarget();
        }
    }

    protected virtual void RecalculatePath()
    {
        if (_navigationAgent != null && IsTargetValid())
        {
            var targetPosition = GetTargetPosition();
            if (targetPosition.HasValue)
            {
                _navigationAgent.TargetPosition = targetPosition.Value;
                _navigationAgent.TargetDesiredDistance = 0.5f; // Smaller target distance
            }
        }
    }
    
    protected virtual void CheckForNearbyObstacles()
    {
        // Check if we're too close to any tower (which might be blocking our path)
        var towers = GetTree().GetNodesInGroup("towers");
        foreach (var towerNode in towers)
        {
            if (towerNode is Tower tower && IsInstanceValid(tower) && tower != _currentTarget)
            {
                float distanceToTower = GlobalPosition.DistanceTo(tower.GlobalPosition);
                if (distanceToTower < 2.0f) // If we're very close to a tower that's not our target
                {
                    // Force path recalculation to avoid getting stuck
                    RecalculatePath();
                    return;
                }
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        OnEnemyDied?.Invoke(this);
        QueueFree();
    }

    protected virtual void OnReachedTarget()
    {
        OnEnemyReachedTarget?.Invoke(this);
        QueueFree();
    }
    
    protected bool IsTargetValid()
    {
        return _currentTarget != null && IsInstanceValid(_currentTarget);
    }
    
    protected Vector3? GetTargetPosition()
    {
        if (IsTargetValid())
        {
            return _currentTarget.GlobalPosition;
        }
        return null;
    }

    public virtual void SetTarget(Node3D target)
    {
        _currentTarget = target;
        if (_navigationAgent != null && target != null && IsInstanceValid(target))
        {
            _navigationAgent.TargetPosition = target.GlobalPosition;
        }
    }

    public virtual bool IsAlive()
    {
        return _currentHealth > 0;
    }

    public virtual int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public virtual int GetMaxHealth()
    {
        return MaxHealth;
    }

    public virtual float GetSpeed()
    {
        return Speed;
    }

    public virtual int GetDamageToTarget()
    {
        return DamageToTarget;
    }
} 