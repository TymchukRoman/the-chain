using Godot;

public partial class Cell : Node3D
{
    public Vector2I GridPosition { get; set; }
    private Label3D _label;
    public bool IsOccupied { get; private set; }
    
    // Tower reference for management
    private Tower _builtTower;
    private HexManagementPanel _managementPanel;
    
    public void MarkOccupied() => IsOccupied = true;
    public void MarkEmpty() => IsOccupied = false;

    public override void _Ready()
    {
        _label = GetNode<Label3D>("Label3D");
        UpdateLabel();
        
        // Get reference to management panel
        _managementPanel = GetNode<HexManagementPanel>("/root/Root/HexManagementPanel");
        
        GD.Print("Cell ready at grid position: " + GridPosition);
    }

    public void UpdateLabel()
    {
        if (_label != null)
            _label.Text = $"({GridPosition.X}, {GridPosition.Y})";
    }
    
    // Tower management methods
    public void SetBuiltTower(Tower tower)
    {
        _builtTower = tower;
        GD.Print("Tower set for cell at: " + GridPosition);
    }
    
    public Tower GetBuiltTower()
    {
        return _builtTower;
    }
    
    public void RemoveTower()
    {
        _builtTower = null;
        GD.Print("Tower removed from cell at: " + GridPosition);
    }
    
    // Right-click handling
    public void OnRightClick()
    {
        GD.Print("Right-click on cell at: " + GridPosition);
        
        if (_managementPanel != null)
        {
            GD.Print("Management panel found, calling ShowForCell");
            _managementPanel.ShowForCell(this);
        }
        else
        {
            GD.PrintErr("Management panel not found!");
        }
    }
}