using Godot;

public partial class Cell : Node3D
{
    public Vector2I GridPosition { get; set; }
    private Label3D _label;
    public bool IsOccupied { get; private set; }
    
    // Tower reference for management
    private Tower _builtTower;
    private HexManagementPanel _managementPanel;
    
    // Highlighting
    private MeshInstance3D _highlightMesh;
    private StandardMaterial3D _highlightMaterial;
    
    public void MarkOccupied() => IsOccupied = true;
    public void MarkEmpty() => IsOccupied = false;

    public override void _Ready()
    {
        _label = GetNode<Label3D>("Label3D");
        UpdateLabel();
        
        // Get reference to management panel
        _managementPanel = GetNode<HexManagementPanel>("/root/Root/HexManagementPanel");
        
        // Setup highlighting
        SetupHighlight();
        
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
    
    // Highlighting methods
    private void SetupHighlight()
    {
        // Create highlight mesh (hex-shaped to match the cell exactly)
        _highlightMesh = new MeshInstance3D();
        var highlightCylinder = new CylinderMesh();
        highlightCylinder.RadialSegments = 6; // Hex shape
        highlightCylinder.CapBottom = false;
        highlightCylinder.CapTop = true; // Enable top cap for top surface highlighting
        highlightCylinder.Height = 0.1f;
        highlightCylinder.TopRadius = 0.8f; // Slightly smaller than cell for better visual
        highlightCylinder.BottomRadius = 0.8f; // Slightly smaller than cell for better visual
        
        // Create highlight material
        _highlightMaterial = new StandardMaterial3D();
        _highlightMaterial.AlbedoColor = new Color(0.2f, 0.8f, 1.0f, 0.6f); // Blue with transparency
        _highlightMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        _highlightMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        _highlightMaterial.EmissionEnabled = true;
        _highlightMaterial.Emission = new Color(0.2f, 0.8f, 1.0f, 0.3f); // Glowing effect
        
        _highlightMesh.Mesh = highlightCylinder;
        _highlightMesh.MaterialOverride = _highlightMaterial;
        
        // Position and rotate to match the cell mesh exactly
        _highlightMesh.Position = new Vector3(0, 0.05f, 0);
        _highlightMesh.Transform = new Transform3D(1.55885f, 0, 0.9f, 0, 0.05f, 0, -0.9f, 0, 1.55885f, 0, 0.05f, 0);
        
        // Initially hidden
        _highlightMesh.Visible = false;
        
        AddChild(_highlightMesh);
    }
    
    public void SetHighlighted(bool highlighted)
    {
        if (_highlightMesh != null)
        {
            _highlightMesh.Visible = highlighted;
        }
    }
}