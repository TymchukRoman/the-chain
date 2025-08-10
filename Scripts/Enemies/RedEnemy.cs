using Godot;

public partial class RedEnemy : Enemy
{
    protected override void SetupTargetPriority()
    {
        // Red enemies prioritize castle
        _targetPriorityList = new string[] { "castle" };
    }

    protected override void SetupAttackType()
    {
        // Red enemies use hit-and-die attack type
        _attackType = AttackType.HitAndDie;
    }

    protected override void OnReachedTarget()
    {
        // Deal damage to castle (if castle has health system)
        if (_currentTarget != null && IsInstanceValid(_currentTarget))
        {
            // You can add castle damage logic here
            // For now, just call the base method which will handle the event
            base.OnReachedTarget();
        }
        
        QueueFree();
    }

    protected override void SetupHealthBar()
    {
        base.SetupHealthBar();
        
        // Set red color for this enemy type
        if (_healthBar != null)
        {
            _healthBar.SetCustomColor(new Color(1.0f, 0.2f, 0.2f, 0.9f)); // Red
        }
    }

    protected override void UpdateHealthBar()
    {
        base.UpdateHealthBar();
        
        // Keep red color for this enemy type
        if (_healthBar != null)
        {
            float healthPercentage = (float)_currentHealth / MaxHealth;
            if (healthPercentage > 0.6f)
            {
                _healthBar.SetCustomColor(new Color(1.0f, 0.2f, 0.2f, 0.9f)); // Red
            }
            else if (healthPercentage > 0.3f)
            {
                _healthBar.SetCustomColor(new Color(1.0f, 0.5f, 0.2f, 0.9f)); // Orange
            }
            else
            {
                _healthBar.SetCustomColor(new Color(0.8f, 0.1f, 0.1f, 0.9f)); // Dark red
            }
        }
    }
} 