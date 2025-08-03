using Godot;
using System.Collections.Generic;

public partial class BlueEnemy : Enemy
{
    private Tower _targetTower;

    public override void _Ready()
    {
        base._Ready();
        FindTargetTower();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        // Check if we need to find a new target
        bool needNewTarget = false;
        
        if (_targetTower == null)
        {
            needNewTarget = true;
        }
        else if (!IsInstanceValid(_targetTower))
        {
            needNewTarget = true;
        }
        else if (!_targetTower.IsInsideTree())
        {
            needNewTarget = true;
        }
        else if (_targetTower.GetCurrentHealth() <= 0)
        {
            needNewTarget = true;
        }
        
        if (needNewTarget)
        {
            _targetTower = null;
            FindTargetTower();
        }
    }

    private void FindTargetTower()
    {
        // Get all towers in the scene
        var towers = GetTree().GetNodesInGroup("towers");
        
        // Filter out invalid towers
        var validTowers = new List<Tower>();
        
        foreach (var towerNode in towers)
        {
            if (towerNode is Tower tower && IsInstanceValid(tower) && tower.IsInsideTree() && tower.GetCurrentHealth() > 0)
            {
                validTowers.Add(tower);
            }
        }
        
        if (validTowers.Count == 0)
        {
            // No valid towers found, target castle instead
            var castle = GetNode<Node3D>("/root/Root/Map/Castle");
            if (castle != null)
            {
                _targetTower = null;
                SetTarget(castle);
            }
            return;
        }

        // Find the closest tower - sort by distance to ensure we get the actual closest
        var towersWithDistance = new List<(Tower tower, float distance)>();
        
        foreach (var tower in validTowers)
        {
            float distance = GlobalPosition.DistanceTo(tower.GlobalPosition);
            towersWithDistance.Add((tower, distance));
        }
        
        // Sort by distance to get the closest tower
        towersWithDistance.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        if (towersWithDistance.Count > 0)
        {
            var closestTower = towersWithDistance[0].tower;
            var closestDistance = towersWithDistance[0].distance;
            
            _targetTower = closestTower;
            SetTarget(closestTower);
            return;
        }


    }

    protected override void OnReachedTarget()
    {
        
        // Deal damage to the tower if it's still valid
        if (_targetTower != null && IsInstanceValid(_targetTower) && _targetTower.IsInsideTree())
        {
            _targetTower.TakeDamage(DamageToTarget);
        }
        
        base.OnReachedTarget();
        QueueFree();
    }

    protected override void SetupHealthBar()
    {
        base.SetupHealthBar();
        
        // Set blue color for this enemy type
        if (_healthBar != null)
        {
            _healthBar.SetCustomColor(new Color(0.2f, 0.2f, 1.0f, 0.9f)); // Blue
        }
    }

    protected override void UpdateHealthBar()
    {
        base.UpdateHealthBar();
        
        // Keep blue color for this enemy type
        if (_healthBar != null)
        {
            float healthPercentage = (float)_currentHealth / MaxHealth;
            if (healthPercentage > 0.6f)
            {
                _healthBar.SetCustomColor(new Color(0.2f, 0.2f, 1.0f, 0.9f)); // Blue
            }
            else if (healthPercentage > 0.3f)
            {
                _healthBar.SetCustomColor(new Color(0.2f, 0.5f, 1.0f, 0.9f)); // Light blue
            }
            else
            {
                _healthBar.SetCustomColor(new Color(0.1f, 0.1f, 0.8f, 0.9f)); // Dark blue
            }
        }
    }
} 