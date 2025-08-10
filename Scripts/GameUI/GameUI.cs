using Godot;
using System.Collections.Generic;

public partial class GameUI : CanvasLayer
{
    private Label _rankLabel;
    private Label _woodLabel;
    private Label _peopleLabel;
    private Label _stoneLabel;
    private Label _waveLabel;
    private Label _supplyTimerLabel;


    public override void _Ready()
    {
        _rankLabel = GetNode<Label>("Panel/HBoxContainer/Rank");
        _woodLabel = GetNode<Label>("Panel/HBoxContainer/Wood");
        _peopleLabel = GetNode<Label>("Panel/HBoxContainer/People");
        _stoneLabel = GetNode<Label>("Panel/HBoxContainer/Stone");
        _waveLabel = GetNode<Label>("Panel/HBoxContainer/Wave");
        _supplyTimerLabel = GetNode<Label>("Panel/HBoxContainer/SupplyTimer");
    }

    public void UpdateUI(int rp, int rank, Dictionary<string, int> resources, int currentWave = 1, float timeUntilSupply = 0.0f)
    {
        if (_rankLabel != null)
            _rankLabel.Text = $"RP: {rp} | Rank: {rank}";
        
        if (_woodLabel != null)
            _woodLabel.Text = $"Wood: {resources[GameConstants.WOOD]}";
        
        if (_peopleLabel != null)
            _peopleLabel.Text = $"People: {resources[GameConstants.PEOPLE]}";
        
        if (_stoneLabel != null)
            _stoneLabel.Text = $"Stone: {resources[GameConstants.STONE]}";
        
        if (_waveLabel != null)
            _waveLabel.Text = $"Wave: {currentWave}";
        
        if (_supplyTimerLabel != null)
        {
            // Ensure time is valid and handle edge cases
            float displayTime = Mathf.Max(0.0f, timeUntilSupply);
            int minutes = (int)(displayTime / 60);
            int seconds = (int)(displayTime % 60);
            
            // Ensure seconds don't go below 0 or above 59
            seconds = Mathf.Clamp(seconds, 0, 59);
            
            _supplyTimerLabel.Text = $"Supply: {minutes:00}:{seconds:00}";
        }
    }
}
