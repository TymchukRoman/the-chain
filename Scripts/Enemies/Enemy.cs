using Godot;
using System;
using System.Collections.Generic;

public enum AttackType
{
    HitAndDie,      // Enemy attacks once and dies
    Continuous      // Enemy continuously attacks until killed
}

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
    
    // Priority-based targeting system
    protected string[] _targetPriorityList;
    protected float _targetSearchTimer = 0.0f;
    protected const float TARGET_SEARCH_INTERVAL = 0.5f; // Search for new targets every 0.5 seconds
    
    // Attack system
    protected AttackType _attackType = AttackType.HitAndDie;
    protected float _attackTimer = 0.0f;
    protected float _attackCooldown = 1.0f; // Time between attacks for continuous enemies
    protected bool _isAttacking = false;
    protected const float ATTACK_RANGE = 1.5f; // Distance at which enemy can attack
    
    // Events
    public delegate void EnemyDiedHandler(Enemy enemy);
    public event EnemyDiedHandler OnEnemyDied;
    
    public delegate void EnemyReachedTargetHandler(Enemy enemy);
    public event EnemyReachedTargetHandler OnEnemyReachedTarget;

    public override void _Ready()
    {
        SetupNavigationAgent();
        SetupHealthBar();
        SetupTargetPriority();
        SetupAttackType();
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
        UpdateTargetSearch((float)delta);
        UpdateMovement((float)delta);
        UpdateAttack((float)delta);
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

    protected virtual void SetupTargetPriority()
    {
        // Override this in derived classes to set the target priority list
        // Example: _targetPriorityList = new string[] { "castle" };
        _targetPriorityList = new string[] { "castle" }; // Default priority
    }

    protected virtual void SetupAttackType()
    {
        // Override this in derived classes to set the attack type
        _attackType = AttackType.HitAndDie; // Default attack type
    }

    protected virtual void UpdateTargetSearch(float delta)
    {
        if (_targetPriorityList == null || _targetPriorityList.Length == 0) return;
        
        _targetSearchTimer += delta;
        if (_targetSearchTimer >= TARGET_SEARCH_INTERVAL)
        {
            _targetSearchTimer = 0.0f;
            
            // Check if current target is still valid
            if (!IsTargetValid())
            {
                FindNewTarget();
            }
        }
    }

    protected virtual void FindNewTarget()
    {
        if (_targetPriorityList == null || _targetPriorityList.Length == 0) return;
        
        // Try to find a target based on priority list
        foreach (string targetType in _targetPriorityList)
        {
            Node3D target = FindTargetByType(targetType);
            if (target != null)
            {
                SetTarget(target);
                return;
            }
        }
        
        // If no target found, the enemy will be stuck
        GD.Print($"Enemy {Name} could not find any valid target from priority list: [{string.Join(", ", _targetPriorityList)}]");
    }

    protected virtual Node3D FindTargetByType(string targetType)
    {
        switch (targetType.ToLower())
        {
            case "castle":
                return FindClosestCastle();
            case "tower":
                return FindClosestTower();
            default:
                GD.PrintErr($"Unknown target type: {targetType}");
                return null;
        }
    }

    protected virtual Node3D FindClosestCastle()
    {
        var castle = GetNode<Node3D>("/root/Root/Map/Castle");
        if (castle != null && IsInstanceValid(castle))
        {
            return castle;
        }
        return null;
    }

    protected virtual Node3D FindClosestTower()
    {
        var towers = GetTree().GetNodesInGroup("towers");
        var validTowers = new List<Tower>();
        
        foreach (var towerNode in towers)
        {
            if (towerNode is Tower tower && IsInstanceValid(tower) && tower.IsInsideTree() && tower.GetCurrentHealth() > 0)
            {
                validTowers.Add(tower);
            }
        }
        
        if (validTowers.Count == 0) return null;
        
        // Find the closest tower
        Tower closestTower = null;
        float closestDistance = float.MaxValue;
        
        foreach (var tower in validTowers)
        {
            float distance = GlobalPosition.DistanceTo(tower.GlobalPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = tower;
            }
        }
        
        return closestTower;
    }

    protected virtual void UpdateHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(_currentHealth, MaxHealth);
        }
    }

    protected virtual void UpdateAttack(float delta)
    {
        if (_attackType != AttackType.Continuous || _currentTarget == null) return;
        
        if (_isAttacking)
        {
            _attackTimer += delta;
            if (_attackTimer >= _attackCooldown)
            {
                _attackTimer = 0.0f;
                PerformAttack();
            }
        }
        else
        {
            // Check if we're close enough to attack
            if (IsTargetValid())
            {
                float distanceToTarget = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition);
                if (distanceToTarget <= ATTACK_RANGE)
                {
                    StartAttacking();
                }
            }
        }
    }

    protected virtual void StartAttacking()
    {
        _isAttacking = true;
        _attackTimer = 0.0f;
        // Stop movement when attacking
        if (_navigationAgent != null)
        {
            _navigationAgent.TargetPosition = GlobalPosition;
        }
    }

    protected virtual void StopAttacking()
    {
        _isAttacking = false;
        _attackTimer = 0.0f;
        // Resume movement
        if (_navigationAgent != null && IsTargetValid())
        {
            var targetPosition = GetTargetPosition();
            if (targetPosition.HasValue)
            {
                _navigationAgent.TargetPosition = targetPosition.Value;
            }
        }
    }

    protected virtual void PerformAttack()
    {
        if (!IsTargetValid()) return;
        
        // Deal damage to the target
        if (_currentTarget is Tower tower)
        {
            tower.TakeDamage(DamageToTarget);
        }
        else if (_currentTarget.Name == "Castle")
        {
            // Deal damage to castle (you can add castle damage logic here)
            GD.Print($"Enemy {Name} dealt {DamageToTarget} damage to castle");
        }
        
        // Check if target is still alive
        if (_currentTarget is Tower targetTower && targetTower.GetCurrentHealth() <= 0)
        {
            // Target destroyed, find new target
            StopAttacking();
            FindNewTarget();
        }
    }

    protected virtual void UpdateMovement(float delta)
    {
        if (_navigationAgent == null || _currentTarget == null) return;

        // If we're attacking, don't move
        if (_isAttacking) return;

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
        if (_attackType == AttackType.HitAndDie)
        {
            // Hit-and-die enemies attack once and then die
            PerformAttack();
            OnEnemyReachedTarget?.Invoke(this);
            QueueFree();
        }
        else if (_attackType == AttackType.Continuous)
        {
            // Continuous enemies start attacking and stay alive
            StartAttacking();
            // Don't die, just start attacking
        }
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