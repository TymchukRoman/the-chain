using Godot;

public partial class BlueEnemy : Enemy
{
    protected override void SetupTargetPriority()
    {
        // Blue enemies prioritize towers, then castle as fallback
        _targetPriorityList = new string[] { "tower", "castle" };
    }

    protected override void SetupAttackType()
    {
        // Blue enemies use continuous attack type
        _attackType = AttackType.Continuous;
        _attackCooldown = 1.5f; // Slightly slower attack rate for balance
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
                _healthBar.SetCustomColor(new Color(0.2f, 0.2f, 0.9f, 0.9f)); // Blue
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