using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Node3D
{
    [Export] public PackedScene CellScene;
    [Export] public float HexRadius = 1.0f;

    [Export] public Vector3 MapMin = new Vector3(-25, 0, -25);
    [Export] public Vector3 MapMax = new Vector3(25, 0, 25);

    public Dictionary<Vector2I, Cell> Cells = new();

    public override void _Ready()
    {
        GenerateHexGrid();
    }

    private void GenerateHexGrid()
    {
        float hexWidth = HexRadius * 2f;
        float hexHeight = Mathf.Sqrt(3) * HexRadius;

        int minQ = Mathf.FloorToInt(MapMin.X / (hexWidth * 0.75f));
        int maxQ = Mathf.CeilToInt(MapMax.X / (hexWidth * 0.75f));
        int minR = Mathf.FloorToInt(MapMin.Z / hexHeight);
        int maxR = Mathf.CeilToInt(MapMax.Z / hexHeight);

        for (int q = minQ; q <= maxQ; q++)
        {
            for (int r = minR; r <= maxR; r++)
            {
                float x = q * (hexWidth * 0.75f);
                float z = hexHeight * (r + 0.5f * (q % 2));

                Vector3 worldPos = new Vector3(x, 0, z);

                if (worldPos.X < MapMin.X || worldPos.X > MapMax.X || worldPos.Z < MapMin.Z || worldPos.Z > MapMax.Z)
                    continue;

                var instance = CellScene.Instantiate<Cell>();
                AddChild(instance);
                instance.Position = worldPos;

                Vector2I gridPos = new Vector2I(q, r);
                instance.GridPosition = gridPos;

                Cells[gridPos] = instance;

                instance.UpdateLabel();
            }
        }
    }

    public Cell GetCell(Vector2I gridPosition)
    {
        return Cells.TryGetValue(gridPosition, out var cell) ? cell : null;
    }
}
