using Godot;
using System;
using System.Collections.Generic;

public partial class Tower : Node3D
{
    [Export] public float Range = 5.0f;
    [Export] public float FireRate = 1.0f; // shots per second
    [Export] public int Damage = 10;
    [Export] public PackedScene ProjectileScene;
    
    private float _fireTimer = 0.0f;
    private Enemy _currentTarget;
    private List<Enemy> _enemiesInRange = new List<Enemy>();
    private Area3D _detectionArea;
    
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
        
        GD.Print("Tower initialized with range: " + Range);
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
        if (body is Enemy enemy)
        {
            GD.Print("Tower detected enemy entering range!");
            _enemiesInRange.Add(enemy);
            if (_currentTarget == null)
            {
                FindNewTarget();
            }
        }
    }
    
    private void OnEnemyExited(Node3D body)
    {
        if (body is Enemy enemy)
        {
            GD.Print("Tower detected enemy leaving range!");
            _enemiesInRange.Remove(enemy);
            if (_currentTarget == enemy)
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
}
