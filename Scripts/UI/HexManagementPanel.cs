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
    private ResourceManager _resourceManager;
    
    // Update timer for real-time resource updates
    private Timer _updateTimer;
    
    // Constants - now using GameConstants
    
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
        _resourceManager = GetNode<ResourceManager>("/root/Root/ResourceManager");
        
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
        
        // Unhighlight previous cell
        if (_currentCell != null)
        {
            _currentCell.SetHighlighted(false);
        }
        
        _currentCell = cell;
        
        // Highlight new cell
        if (_currentCell != null)
        {
            _currentCell.SetHighlighted(true);
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
        }
        else
        {
        }
        
        UpdatePanel();
        
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
        
    }
    
    private void PositionNearCell(Cell cell)
    {
        // Fixed position in top right corner
        var viewport = GetViewport();
        var viewportSize = viewport.GetVisibleRect().Size;
        
        
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
        
        // Update build button cost using GameConstants
        var buildCosts = GameConstants.TOWER_COSTS[0]; // Level 0 -> 1 (build cost)
        string buttonText = "Build Tower (";
        bool canAfford = true;
        
        foreach (var cost in buildCosts)
        {
            if (!_resourceManager.HasResource(cost.Key, cost.Value))
            {
                canAfford = false;
            }
            buttonText += $"{cost.Value} {cost.Key}, ";
        }
        buttonText = buttonText.TrimEnd(',', ' ') + ")";
        _towerButton.Disabled = !canAfford;
        
        if (!canAfford)
        {
            buttonText += " (Insufficient resources)";
        }
        
        // Apply text truncation
        _towerButton.Text = TruncateText(buttonText, 35);
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
                var upgradeCosts = GameConstants.TOWER_COSTS[tower.GetCurrentLevel()];
                string upgradeText = "Upgrade (";
                bool canAfford = true;
                
                foreach (var cost in upgradeCosts)
                {
                    if (!_resourceManager.HasResource(cost.Key, cost.Value))
                    {
                        canAfford = false;
                    }
                    upgradeText += $"{cost.Value} {cost.Key}, ";
                }
                upgradeText = upgradeText.TrimEnd(',', ' ') + ")";
                
                _upgradeButton.Disabled = !canAfford;
                
                if (!canAfford)
                {
                    upgradeText += " (Insufficient resources)";
                }
                _upgradeButton.Text = TruncateText(upgradeText, 30);
            }
            else
            {
                _upgradeButton.Text = "Upgrade (Max Level)";
                _upgradeButton.Disabled = true;
            }
            
            // Update repair button
            bool needsRepair = tower.GetCurrentHealth() < tower.MaxHealth;
            int repairCost = tower.GetRepairCost(); // Use dynamic repair cost
            string repairText = $"Repair ({repairCost} people)";
            _repairButton.Disabled = !needsRepair || !_resourceManager.HasResource("people", repairCost);
            
            if (!needsRepair)
            {
                repairText = "Repair (Full Health)";
            }
            else if (!_resourceManager.HasResource("people", repairCost))
            {
                repairText += " (Insufficient people)";
            }
            _repairButton.Text = TruncateText(repairText, 30);
        }
    }
    
    private void OnBuildTower()
    {
        if (_currentCell == null) 
        {
            return;
        }
        
        
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
    
    private void OnUpdateTimer()
    {
        // Update panel if visible and has a current cell
        if (Visible && _currentCell != null)
        {
            UpdatePanel();
        }
    }
    
    private string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text;
        
        return text.Substring(0, maxLength - 3) + "...";
    }
    
    public override void _Input(InputEvent @event)
    {
        // Only handle input if panel is visible
        if (!Visible) return;
        
        // Close panel when pressing Escape key
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        // Handle mouse input for clicking outside
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            var backgroundPanel = GetNode<Panel>("Background");
            var panelRect = backgroundPanel.GetRect();
            var localMousePos = backgroundPanel.GetLocalMousePosition();
            
            // Check if click is outside the panel
            if (!panelRect.HasPoint(localMousePos))
            {
                // Use a short delay to ensure button clicks are processed first
                GetTree().CreateTimer(0.05f).Timeout += () => {
                    if (Visible) // Only hide if still visible (button wasn't clicked)
                    {
                        Hide();
                    }
                };
            }
        }
    }
} 