using Godot;
using System;

public partial class MainMenuButton : Button
{
    private const string MainMenuScenePath = "res://Scenes/UI/MainMenu.tscn";

    public override void _Pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }
}