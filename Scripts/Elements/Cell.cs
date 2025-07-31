using Godot;

public partial class Cell : Node3D
{
    public Vector2I GridPosition { get; set; }
    private Label3D _label;
    public bool IsOccupied { get; private set; }
    public void MarkOccupied() => IsOccupied = true;
    public void MarkEmpty() => IsOccupied = false;

    public override void _Ready()
    {
        _label = GetNode<Label3D>("Label3D");
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        if (_label != null)
            _label.Text = $"({GridPosition.X}, {GridPosition.Y})";
    }
}