using Godot;
using System;

public partial class MainMenuButton : Button
{
    // IMPORTANT: Change this path to your actual main menu scene file.
    private const string MainMenuScenePath = "res://Scenes/UI/MainMenu.tscn";

    public override void _Pressed()
    {
        // ALWAYS unpause before changing scenes to avoid the new scene being frozen.
        GetTree().Paused = false;

        // Change to the main menu scene.
        GetTree().ChangeSceneToFile(MainMenuScenePath);
    }
}