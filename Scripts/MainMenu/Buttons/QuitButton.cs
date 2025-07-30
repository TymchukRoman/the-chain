using Godot;
using System;

public partial class QuitButton : Button
{
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
    }
    private void OnButtonPressed()
    {
        GetTree().Quit();
    }
}
