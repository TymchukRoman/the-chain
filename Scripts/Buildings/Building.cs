using Godot;

public abstract partial class Building : Node3D
{
    [Export] public int MaxHealth = 100;
    [Export] public int RepairCost = 3; // Base repair cost in people
    [Export] public int DemolishRefund = 10;
    
    protected int _currentHealth;
    protected Cell _builtOnCell;
    protected Game _game;
    
    // Health bar component
    protected HealthBar _healthBar;

    // Events
    public delegate void BuildingDestroyedHandler(Building building);
    public event BuildingDestroyedHandler OnBuildingDestroyed;

    public override void _Ready()
    {
        _currentHealth = MaxHealth; // Set current health first
        _game = GetNode<Game>("/root/Root");
        SetupHealthBar(); // Setup health bar after health is initialized
    }

    public virtual void Initialize(int health)
    {
        MaxHealth = health;
        _currentHealth = health;
        UpdateHealthBar();
    }

    public virtual void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        GD.Print("Building destroyed!");
        OnBuildingDestroyed?.Invoke(this);
        
        // Clean up cell reference
        if (_builtOnCell != null)
        {
            _builtOnCell.RemoveTower();
        }
        
        QueueFree();
    }

    protected virtual void SetupHealthBar()
    {
        // Create and setup the health bar
        _healthBar = new HealthBar();
        AddChild(_healthBar);
        _healthBar.Initialize(_currentHealth, MaxHealth);
        
        // Buildings should always show their health bar
        _healthBar.SetHealthBarVisible(true);
    }

    protected virtual void UpdateHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateHealth(_currentHealth, MaxHealth);
        }
    }



    public virtual void SetBuiltOnCell(Cell cell)
    {
        _builtOnCell = cell;
    }

    public virtual Cell GetBuiltOnCell()
    {
        return _builtOnCell;
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

    public virtual bool CanRepair()
    {
        return _currentHealth < MaxHealth;
    }

    public virtual int GetRepairCost()
    {
        // Simple repair cost based on missing health
        int healthDifference = MaxHealth - _currentHealth;
        int repairCost = Mathf.Max(1, healthDifference / 10); // 1 people per 10 missing health, minimum 1
        
        return repairCost;
    }
    
    protected virtual void RemoveFromGroups()
    {
        // Override in derived classes to remove from specific groups
    }

    public virtual void Repair()
    {
        if (CanRepair())
        {
            _currentHealth = MaxHealth;
            UpdateHealthBar();
            GD.Print("Building repaired to full health!");
        }
    }

    public virtual void Demolish()
    {
        // Remove from any groups before demolishing
        RemoveFromGroups();
        GD.Print("Building demolished!");
        
        // Clean up cell reference
        if (_builtOnCell != null)
        {
            _builtOnCell.RemoveTower();
        }
        
        QueueFree();
    }

    public virtual Vector3 GetBuildingPosition()
    {
        return GlobalPosition;
    }
} 