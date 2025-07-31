using Godot;
using System;
using System.Collections.Generic;

public partial class Map : Node3D
{
    private Game _game;
    [Export] public PackedScene CellScene;
    [Export] public float HexRadius = 1.0f;

    [Export] public Vector3 MapMin = new Vector3(-25, 0, -25);
    [Export] public Vector3 MapMax = new Vector3(25, 0, 25);

    public Dictionary<Vector2I, Cell> Cells = new();
    private NavigationRegion3D _navigationRegion;

    public override void _Ready()
    {
        GenerateHexGrid();
        SetupNavigationRegion();
        _game = GetParent<Game>();
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

    private void SetupNavigationRegion()
    {
        // Create NavigationRegion3D
        _navigationRegion = new NavigationRegion3D();
        AddChild(_navigationRegion);

        // Create a simple navigation mesh covering the entire map
        var navigationMesh = new NavigationMesh();
        
        // Create vertices for a simple rectangular navigation mesh
        var vertices = new Vector3[]
        {
            new Vector3(MapMin.X, 0, MapMin.Z),
            new Vector3(MapMax.X, 0, MapMin.Z),
            new Vector3(MapMax.X, 0, MapMax.Z),
            new Vector3(MapMin.X, 0, MapMax.Z)
        };

        // Create polygon indices (two triangles forming a rectangle)
        var polygonIndices = new int[]
        {
            0, 1, 2,  // First triangle
            0, 2, 3   // Second triangle
        };

        // Set up the navigation mesh
        navigationMesh.Vertices = vertices;
        navigationMesh.AddPolygon(polygonIndices);

        // Assign the navigation mesh to the region
        _navigationRegion.NavigationMesh = navigationMesh;
    }

    public Cell GetCell(Vector2I gridPosition)
    {
        return Cells.TryGetValue(gridPosition, out var cell) ? cell : null;
    }

    public NavigationRegion3D GetNavigationRegion()
    {
        return _navigationRegion;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            GD.Print("Clicked");
            var viewport = GetViewport();
            var camera = GetViewport().GetCamera3D();
            var from = camera.ProjectRayOrigin(mouseEvent.Position);
            var to = from + camera.ProjectRayNormal(mouseEvent.Position) * 1000;

            var space = GetWorld3D().DirectSpaceState;
            var result = space.IntersectRay(new PhysicsRayQueryParameters3D
            {
                From = from,
                To = to,
                CollisionMask = 1
            });
            GD.Print(result);

            if (result.TryGetValue("collider", out var colliderVariant))
            {
                var collider = colliderVariant.AsGodotObject();

                // Check if clicking on a tower for upgrade
                if (collider is Node3D node)
                {
                    // Check if it's a tower
                    var tower = node as Tower;
                    if (tower == null && node.GetParent() is Tower parentTower)
                    {
                        tower = parentTower;
                    }
                    
                    if (tower != null)
                    {
                        GD.Print("Tower upgrade attempt");
                        if (tower.TryUpgrade(_game))
                        {
                            GD.Print("Tower upgraded successfully!");
                        }
                        else
                        {
                            GD.Print("Tower upgrade failed - insufficient resources or max level reached");
                        }
                        return;
                    }
                    
                    // Check if it's a cell for building
                    if (node.GetParent() is Cell cell)
                    {
                        GD.Print("Build call");
                        _game.TryBuildTowerOn(cell);
                    }
                }
            }
        }
    }
}
