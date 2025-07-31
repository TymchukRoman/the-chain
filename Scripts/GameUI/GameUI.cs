using Godot;
using System.Collections.Generic;

public partial class GameUI : CanvasLayer
{
    private Label _rankLabel;
    private Label _woodLabel;
    private Label _ammoLabel;
    private Label _foodLabel;
    private Label _peopleLabel;
    private Label _waveLabel;
    private Label _supplyTimerLabel;

    public override void _Ready()
    {
        _rankLabel = GetNode<Label>("Panel/HBoxContainer/Rank");
        _woodLabel = GetNode<Label>("Panel/HBoxContainer/Wood");
        _ammoLabel = GetNode<Label>("Panel/HBoxContainer/Ammo");
        _foodLabel = GetNode<Label>("Panel/HBoxContainer/Food");
        _peopleLabel = GetNode<Label>("Panel/HBoxContainer/People");
        _waveLabel = GetNode<Label>("Panel/HBoxContainer/Wave");
        _supplyTimerLabel = GetNode<Label>("Panel/HBoxContainer/SupplyTimer");
    }

    public void UpdateUI(int rp, int rank, Dictionary<string, int> resources, int currentWave = 1, float timeUntilSupply = 0.0f)
    {
        if (_rankLabel != null)
            _rankLabel.Text = $"RP: {rp} | Rank: {rank}";
        
        if (_woodLabel != null)
            _woodLabel.Text = $"Wood: {resources["wood"]}";
        
        if (_ammoLabel != null)
            _ammoLabel.Text = $"Ammo: {resources["ammo"]}";
        
        if (_foodLabel != null)
            _foodLabel.Text = $"Food: {resources["food"]}";
        
        if (_peopleLabel != null)
            _peopleLabel.Text = $"People: {resources["people"]}";
        
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
