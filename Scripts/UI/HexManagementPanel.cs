using Godot;
using System;

public partial class HexManagementPanel : Control
{
    // UI References
    private VBoxContainer _emptyStateContainer;
    private VBoxContainer _occupiedStateContainer;
    private Button _towerButton;
    private ProgressBar _healthBar;
    private Label _healthText;
    private Button _upgradeButton;
    private Button _repairButton;
    private Button _demolishButton;
    private Button _closeButton;
    
    // Current cell being managed
    private Cell _currentCell;
    private Game _game;
    
    // Constants
    private const int TOWER_COST = 20;
    private const int REPAIR_COST = 30;
    private const int UPGRADE_COST = 100;
    
    public override void _Ready()
    {
        // Get UI references
        _emptyStateContainer = GetNode<VBoxContainer>("Background/ContentContainer/EmptyStateContainer");
        _occupiedStateContainer = GetNode<VBoxContainer>("Background/ContentContainer/OccupiedStateContainer");
        _towerButton = GetNode<Button>("Background/ContentContainer/EmptyStateContainer/BuildableList/TowerButton");
        _healthBar = GetNode<ProgressBar>("Background/ContentContainer/OccupiedStateContainer/HealthBar");
        _healthText = GetNode<Label>("Background/ContentContainer/OccupiedStateContainer/HealthText");
        _upgradeButton = GetNode<Button>("Background/ContentContainer/OccupiedStateContainer/UpgradeButton");
        _repairButton = GetNode<Button>("Background/ContentContainer/OccupiedStateContainer/RepairButton");
        _demolishButton = GetNode<Button>("Background/ContentContainer/OccupiedStateContainer/DemolishButton");
        _closeButton = GetNode<Button>("Background/CloseButton");
        
        // Get game reference
        _game = GetNode<Game>("/root/Root");
        
        // Connect button signals
        _towerButton.Pressed += OnBuildTower;
        _upgradeButton.Pressed += OnUpgradeTower;
        _repairButton.Pressed += OnRepairTower;
        _demolishButton.Pressed += OnDemolishTower;
        _closeButton.Pressed += Hide;
        
        // Initially hide the panel
        Hide();
        
        // Set high z-index to ensure panel appears above other UI
        ZIndex = 1000;
        
    }
    
    public void ShowForCell(Cell cell)
    {
        GD.Print("ShowForCell called for cell at: " + cell.GridPosition);
        
        _currentCell = cell;
        PositionNearCell(cell);
        UpdatePanel();
        Show();
        
        GD.Print("Panel should now be visible at position: " + Position);
        GD.Print("Panel Visible property: " + Visible);
        GD.Print("Panel Modulate: " + Modulate);
        GD.Print("Panel Z-Index: " + ZIndex);
    }
    
    public void RefreshPanel()
    {
        if (Visible && _currentCell != null)
        {
            UpdatePanel();
        }
    }
    
    public new void Hide()
    {
        _currentCell = null;
        base.Hide();
        GD.Print("HexManagementPanel hidden");
    }
    
    private void PositionNearCell(Cell cell)
    {
        // Start with center position to test visibility
        var viewport = GetViewport();
        var viewportSize = viewport.GetVisibleRect().Size;
        
        GD.Print("Viewport size: " + viewportSize);
        
        // Position in center of screen for testing
        var x = viewportSize.X / 2 - 150; // Center with 300px width
        var y = viewportSize.Y / 2 - 200; // Center with 400px height
        
        Position = new Vector2(x, y);
        
        GD.Print("Panel positioned at center: " + Position);
    }
    
    private void UpdatePanel()
    {
        if (_currentCell == null) return;
        
        if (_currentCell.IsOccupied)
        {
            ShowOccupiedState();
        }
        else
        {
            ShowEmptyState();
        }
    }
    
    private void ShowEmptyState()
    {
        _emptyStateContainer.Show();
        _occupiedStateContainer.Hide();
        
        // Update build button cost
        _towerButton.Text = $"Build Tower ({TOWER_COST} wood)";
        
        // Enable/disable based on resources
        bool canAfford = _game.HasResource("wood", TOWER_COST);
        _towerButton.Disabled = !canAfford;
        
        if (!canAfford)
        {
            _towerButton.Text += " (Insufficient wood)";
        }
    }
    
    private void ShowOccupiedState()
    {
        _emptyStateContainer.Hide();
        _occupiedStateContainer.Show();
        
        var tower = _currentCell.GetBuiltTower();
        if (tower != null)
        {
            // Update health bar
            float healthPercentage = (float)tower.GetCurrentHealth() / tower.MaxHealth;
            _healthBar.Value = healthPercentage * 100;
            _healthText.Text = $"Health: {tower.GetCurrentHealth()}/{tower.MaxHealth}";
            
            // Update health bar color
            if (healthPercentage > 0.6f)
            {
                _healthBar.Modulate = new Color(0.2f, 1.0f, 0.2f); // Green
            }
            else if (healthPercentage > 0.3f)
            {
                _healthBar.Modulate = new Color(1.0f, 1.0f, 0.2f); // Yellow
            }
            else
            {
                _healthBar.Modulate = new Color(1.0f, 0.2f, 0.2f); // Red
            }
            
            // Update upgrade button
            if (tower.CanUpgrade())
            {
                int upgradeCost = tower.GetUpgradeCost();
                _upgradeButton.Text = $"Upgrade ({upgradeCost} wood)";
                _upgradeButton.Disabled = !_game.HasResource("wood", upgradeCost);
                
                if (!_game.HasResource("wood", upgradeCost))
                {
                    _upgradeButton.Text += " (Insufficient wood)";
                }
            }
            else
            {
                _upgradeButton.Text = "Upgrade (Max Level)";
                _upgradeButton.Disabled = true;
            }
            
            // Update repair button
            bool needsRepair = tower.GetCurrentHealth() < tower.MaxHealth;
            _repairButton.Text = $"Repair ({REPAIR_COST} wood)";
            _repairButton.Disabled = !needsRepair || !_game.HasResource("wood", REPAIR_COST);
            
            if (!needsRepair)
            {
                _repairButton.Text = "Repair (Full Health)";
            }
            else if (!_game.HasResource("wood", REPAIR_COST))
            {
                _repairButton.Text += " (Insufficient wood)";
            }
        }
    }
    
    private void OnBuildTower()
    {
        if (_currentCell == null) return;
        
        _game.BuildTowerOnCell(_currentCell);
        // Keep panel open and refresh it to show occupied state
        UpdatePanel();
    }
    
    private void OnUpgradeTower()
    {
        if (_currentCell == null) return;
        
        var tower = _currentCell.GetBuiltTower();
        if (tower != null)
        {
            if (_game.UpgradeTower(tower))
            {
                UpdatePanel(); // Refresh panel after upgrade
            }
        }
    }
    
    private void OnRepairTower()
    {
        if (_currentCell == null) return;
        
        var tower = _currentCell.GetBuiltTower();
        if (tower != null)
        {
            if (_game.RepairTower(tower))
            {
                UpdatePanel(); // Refresh panel after repair
            }
        }
    }
    
    private void OnDemolishTower()
    {
        if (_currentCell == null) return;
        
        var tower = _currentCell.GetBuiltTower();
        if (tower != null)
        {
            _game.DemolishTower(tower);
            Hide();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // Only handle input if panel is visible
        if (!Visible) return;
        
        // Close panel when clicking outside
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            var panelRect = GetNode<Panel>("Background").GetRect();
            var localMousePos = GetLocalMousePosition();
            
            if (!panelRect.HasPoint(localMousePos))
            {
                Hide();
            }
        }
    }
} 