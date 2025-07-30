using Godot;
using System;

public partial class LoadButton : Button
{
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        GD.Print("Load button");
    }
}
