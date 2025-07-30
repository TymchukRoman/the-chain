using Godot;
using System;

public partial class ContinueButton : Button
{
    public override void _Pressed()
    {
        GetTree().Paused = false;
        GetOwner<InGameMenu>().Hide();
    }
}