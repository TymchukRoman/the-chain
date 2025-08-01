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
    
    // Update timer for real-time resource updates
    private Timer _updateTimer;
    
    // Constants
    private const int TOWER_WOOD_COST = 20; // Tower costs 20 wood
    private const int TOWER_PEOPLE_COST = 5; // Tower costs 5 people
    private const int REPAIR_COST = 3; // Base repair cost in people
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
        
        // Setup update timer for real-time resource updates
        _updateTimer = new Timer();
        _updateTimer.WaitTime = 0.5f; // Update every 0.5 seconds
        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
        
    }
    
    public void ShowForCell(Cell cell)
    {
        GD.Print("=== ShowForCell called for cell at: " + cell.GridPosition + " ===");
        GD.Print("Panel currently visible: " + Visible);
        GD.Print("Current cell: " + (_currentCell != null ? _currentCell.GridPosition.ToString() : "null"));
        
        // Unhighlight previous cell
        if (_currentCell != null)
        {
            _currentCell.SetHighlighted(false);
            GD.Print("Unhighlighted previous cell");
        }
        
        _currentCell = cell;
        GD.Print("Set new current cell");
        
        // Highlight new cell
        if (_currentCell != null)
        {
            _currentCell.SetHighlighted(true);
            GD.Print("Highlighted new cell");
        }
        
        // Only position and show if panel is not already visible
        if (!Visible)
        {
            PositionNearCell(cell);
            Show();
            
            // Start the update timer when panel becomes visible
            if (_updateTimer != null)
            {
                _updateTimer.Start();
            }
            GD.Print("Panel was hidden, now showing");
        }
        else
        {
            GD.Print("Panel was already visible, just updating");
        }
        
        UpdatePanel();
        
        GD.Print("Panel should now be visible at position: " + Position);
        GD.Print("Panel Visible property: " + Visible);
        GD.Print("Panel Modulate: " + Modulate);
        GD.Print("Panel Z-Index: " + ZIndex);
        GD.Print("=== End ShowForCell ===");
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
        // Unhighlight current cell
        if (_currentCell != null)
        {
            _currentCell.SetHighlighted(false);
        }
        
        _currentCell = null;
        base.Hide();
        
        // Stop the update timer when panel is hidden
        if (_updateTimer != null)
        {
            _updateTimer.Stop();
        }
        
        GD.Print("HexManagementPanel hidden");
    }
    
    private void PositionNearCell(Cell cell)
    {
        // Fixed position in top right corner
        var viewport = GetViewport();
        var viewportSize = viewport.GetVisibleRect().Size;
        
        GD.Print("Viewport size: " + viewportSize);
        
        // Get the background panel
        var backgroundPanel = GetNode<Panel>("Background");
        
        // Set the background panel to top-right position
        backgroundPanel.AnchorLeft = 1.0f;
        backgroundPanel.AnchorTop = 0.0f;
        backgroundPanel.AnchorRight = 1.0f;
        backgroundPanel.AnchorBottom = 0.0f;
        
        // Set offset to position it in top-right corner
        backgroundPanel.OffsetLeft = -320; // 300 width + 20 margin
        backgroundPanel.OffsetTop = 20;
        backgroundPanel.OffsetRight = -20;
        backgroundPanel.OffsetBottom = 420; // 400 height + 20 margin
        
        GD.Print("Background panel positioned at top-right with anchors");
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
        _towerButton.Text = $"Build Tower ({TOWER_WOOD_COST} wood, {TOWER_PEOPLE_COST} people)";
        
        // Enable/disable based on resources
        bool canAffordWood = _game.HasResource("wood", TOWER_WOOD_COST);
        bool canAffordPeople = _game.HasResource("people", TOWER_PEOPLE_COST);
        bool canAfford = canAffordWood && canAffordPeople;
        _towerButton.Disabled = !canAfford;
        
        if (!canAfford)
        {
            if (!canAffordWood && !canAffordPeople)
            {
                _towerButton.Text += " (Insufficient wood & people)";
            }
            else if (!canAffordWood)
            {
                _towerButton.Text += " (Insufficient wood)";
            }
            else
            {
                _towerButton.Text += " (Insufficient people)";
            }
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
                _upgradeButton.Text = $"Upgrade ({upgradeCost} people)";
                _upgradeButton.Disabled = !_game.HasResource("people", upgradeCost);
                
                if (!_game.HasResource("people", upgradeCost))
                {
                    _upgradeButton.Text += " (Insufficient people)";
                }
            }
            else
            {
                _upgradeButton.Text = "Upgrade (Max Level)";
                _upgradeButton.Disabled = true;
            }
            
            // Update repair button
            bool needsRepair = tower.GetCurrentHealth() < tower.MaxHealth;
            int repairCost = tower.GetRepairCost(); // Use dynamic repair cost
            _repairButton.Text = $"Repair ({repairCost} people)";
            _repairButton.Disabled = !needsRepair || !_game.HasResource("people", repairCost);
            
            if (!needsRepair)
            {
                _repairButton.Text = "Repair (Full Health)";
            }
            else if (!_game.HasResource("people", repairCost))
            {
                _repairButton.Text += " (Insufficient people)";
            }
        }
    }
    
    private void OnBuildTower()
    {
        GD.Print("=== OnBuildTower called! ===");
        if (_currentCell == null) 
        {
            GD.Print("ERROR: Current cell is null!");
            return;
        }
        
        GD.Print("Building tower on cell at: " + _currentCell.GridPosition);
        GD.Print("Cell occupied: " + _currentCell.IsOccupied);
        
        _game.BuildTowerOnCell(_currentCell);
        
        // Keep panel open and refresh it to show occupied state
        UpdatePanel();
        GD.Print("Tower building attempt completed");
        GD.Print("=== End OnBuildTower ===");
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
    
    private void OnUpdateTimer()
    {
        // Update panel if visible and has a current cell
        if (Visible && _currentCell != null)
        {
            UpdatePanel();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // Only handle input if panel is visible
        if (!Visible) return;
        
        // Close panel when clicking outside
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            var backgroundPanel = GetNode<Panel>("Background");
            var panelRect = backgroundPanel.GetRect();
            var localMousePos = backgroundPanel.GetLocalMousePosition();
            
            // Check if click is outside the panel
            if (!panelRect.HasPoint(localMousePos))
            {
                // Use a longer delay to ensure button clicks are processed
                GetTree().CreateTimer(0.2f).Timeout += () => {
                    if (Visible) // Only hide if still visible (button wasn't clicked)
                    {
                        Hide();
                    }
                };
            }
        }
    }
} 