using Godot;
using System;

public partial class StartButton : Button
{
    public override void _Ready()
    {
        Pressed += OnButtonPressed;
        GrabFocus();
    }

    private void OnButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
    }
}
