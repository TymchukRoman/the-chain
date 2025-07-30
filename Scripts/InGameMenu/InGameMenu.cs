using Godot;
using System;

public partial class InGameMenu : CanvasLayer
{
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (GetTree().Paused)
            {
                GetTree().Paused = false;
                Hide();
            }
            else
            {
                GetTree().Paused = true;
                Show();
            }
        }
    }
}