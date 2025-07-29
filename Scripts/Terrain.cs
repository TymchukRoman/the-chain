using Godot;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
public partial class Terrain : Node3D
{
    private const float MinDistance = 20f;
    private const int MaxAttempts = 50;
    private List<Node3D> trees = [];

    public override async void _Ready()
    {
        var tree = ResourceLoader.Load<PackedScene>("res://Scenes/tree.tscn");
        var random = new Random();
        var spawnedPositions = new List<Vector3>();
        while (true)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector3 candidatePos;
                int attempts = 0;

                do
                {
                    candidatePos = new Vector3(
                        random.Next(-450, 450),
                        1,
                        random.Next(-450, 450)
                    );

                    attempts++;
                    if (attempts > MaxAttempts)
                    {
                        GD.Print("Failed to find non-overlapping position for tree ", i);
                        break;
                    }

                } while (IsTooClose(candidatePos, spawnedPositions));

                var newTree = tree.Instantiate<Node3D>();
                AddChild(newTree);
                newTree.GlobalPosition = candidatePos;
                newTree.Scale *= 50;

                spawnedPositions.Add(candidatePos);

            }
            await Task.Delay(2000);
        }
    }

    private bool IsTooClose(Vector3 candidate, List<Vector3> existingPositions)
    {
        foreach (var pos in existingPositions)
        {
            if (candidate.DistanceTo(pos) < MinDistance)
                return true;
        }
        return false;
    }
}