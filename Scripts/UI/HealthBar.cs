using Godot;

public partial class HealthBar : Node3D
{
    [Export] public Vector3 Offset = new Vector3(0, 1.5f, 0);
    [Export] public int FontSize = 24;
    [Export] public float PixelSize = 0.02f;
    [Export] public bool ShowText = true;
    [Export] public bool AutoRotateToCamera = true;
    
    // Health bar components (like the old implementation)
    private Node3D _healthBarContainer;
    private MeshInstance3D _healthBarBackground;
    private MeshInstance3D _healthBarFill;
    private Label3D _healthTextLabel;
    private bool _healthBarVisible = false;
    
    private int _currentHealth;
    private int _maxHealth;
    
    public delegate void HealthBarVisibilityChangedHandler(bool visible);
    public event HealthBarVisibilityChangedHandler OnVisibilityChanged;
    
    public override void _Ready()
    {
        SetupHealthBar();
    }
    
    public override void _Process(double delta)
    {
        if (AutoRotateToCamera && _healthBarVisible)
        {
            UpdateHealthBarRotation();
        }
    }
    
    private void SetupHealthBar()
    {
        // Create health bar container
        _healthBarContainer = new Node3D();
        AddChild(_healthBarContainer);
        _healthBarContainer.Position = Offset;
        
        // Create background bar (larger and more visible)
        var backgroundMesh = new BoxMesh();
        backgroundMesh.Size = new Vector3(2.0f, 0.2f, 0.1f); // Much larger
        var backgroundMaterial = new StandardMaterial3D();
        backgroundMaterial.AlbedoColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Darker, fully opaque
        backgroundMaterial.EmissionEnabled = true;
        backgroundMaterial.Emission = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Slight glow
        backgroundMesh.Material = backgroundMaterial;
        
        _healthBarBackground = new MeshInstance3D();
        _healthBarBackground.Mesh = backgroundMesh;
        _healthBarContainer.AddChild(_healthBarBackground);
        
        // Create fill bar (larger and brighter)
        var fillMesh = new BoxMesh();
        fillMesh.Size = new Vector3(1.8f, 0.16f, 0.08f); // Larger fill
        var fillMaterial = new StandardMaterial3D();
        fillMaterial.AlbedoColor = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Bright green, fully opaque
        fillMaterial.EmissionEnabled = true;
        fillMaterial.Emission = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Glowing effect
        fillMesh.Material = fillMaterial;
        
        _healthBarFill = new MeshInstance3D();
        _healthBarFill.Mesh = fillMesh;
        _healthBarFill.Position = new Vector3(0, 0, 0.02f); // Slightly in front
        _healthBarContainer.AddChild(_healthBarFill);
        
        // Create health text label
        if (ShowText)
        {
            _healthTextLabel = new Label3D();
            _healthTextLabel.Text = "100/100";
            _healthTextLabel.FontSize = FontSize;
            _healthTextLabel.PixelSize = PixelSize;
            _healthTextLabel.Position = new Vector3(0, 0.3f, 0.03f); // Above the health bar
            _healthTextLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _healthTextLabel.VerticalAlignment = VerticalAlignment.Center;
            _healthBarContainer.AddChild(_healthTextLabel);
        }
        
        // Make health bar always face camera (using Sprite3D for billboard effect)
        var billboardSprite = new Sprite3D();
        billboardSprite.PixelSize = 0.01f;
        billboardSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        billboardSprite.Visible = false; // We'll use this for billboard effect
        _healthBarContainer.AddChild(billboardSprite);
        
        // Initially hide health bar
        _healthBarContainer.Visible = false;
    }
    
    public void Initialize(int currentHealth, int maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;
        
        // Ensure we have valid health values
        if (_maxHealth <= 0) _maxHealth = 100;
        if (_currentHealth <= 0) _currentHealth = _maxHealth;
        
        UpdateHealthBar();
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth = -1)
    {
        _currentHealth = currentHealth;
        if (maxHealth > 0)
        {
            _maxHealth = maxHealth;
        }
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        if (_healthBarContainer == null) return;
        
        float healthPercentage = (float)_currentHealth / _maxHealth;
        
        // Ensure health percentage is valid
        healthPercentage = Mathf.Clamp(healthPercentage, 0.0f, 1.0f);
        
        // Update fill bar width based on health percentage
        var fillMesh = _healthBarFill.Mesh as BoxMesh;
        if (fillMesh != null)
        {
            fillMesh.Size = new Vector3(1.8f * healthPercentage, 0.16f, 0.08f); // Larger size
        }
        
        // Update fill bar color based on health (brighter colors)
        var fillMaterial = _healthBarFill.GetActiveMaterial(0) as StandardMaterial3D;
        if (fillMaterial != null)
        {
            if (healthPercentage > 0.6f)
            {
                fillMaterial.AlbedoColor = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Bright green
                fillMaterial.Emission = new Color(0.2f, 1.0f, 0.2f, 1.0f);
            }
            else if (healthPercentage > 0.3f)
            {
                fillMaterial.AlbedoColor = new Color(1.0f, 1.0f, 0.2f, 1.0f); // Bright yellow
                fillMaterial.Emission = new Color(1.0f, 1.0f, 0.2f, 1.0f);
            }
            else
            {
                fillMaterial.AlbedoColor = new Color(1.0f, 0.2f, 0.2f, 1.0f); // Bright red
                fillMaterial.Emission = new Color(1.0f, 0.2f, 0.2f, 1.0f);
            }
        }
        
        // Update health text
        if (_healthTextLabel != null)
        {
            _healthTextLabel.Text = _currentHealth + "/" + _maxHealth;
        }
        
        // Show health bar when damaged
        if (!_healthBarVisible && _currentHealth < _maxHealth)
        {
            _healthBarVisible = true;
            _healthBarContainer.Visible = true;
        }
    }
    
    private void UpdateHealthBarRotation()
    {
        if (_healthBarContainer == null) return;
        
        // Get the camera
        var camera = GetViewport().GetCamera3D();
        if (camera == null) return;
        
        // Get direction from health bar to camera (only X and Z components to keep it horizontal)
        var healthBarPos = _healthBarContainer.GlobalPosition;
        var cameraPos = camera.GlobalPosition;
        var direction = new Vector3(cameraPos.X - healthBarPos.X, 0, cameraPos.Z - healthBarPos.Z).Normalized();
        
        // Calculate Y rotation only (no X or Z rotation to keep it horizontal)
        var targetYRotation = Mathf.Atan2(direction.X, direction.Z);
        
        // Apply only Y rotation to keep health bar horizontal
        _healthBarContainer.GlobalRotation = new Vector3(0, targetYRotation, 0);
    }
    
    public void SetHealthBarVisible(bool visible)
    {
        _healthBarVisible = visible;
        if (_healthBarContainer != null)
        {
            _healthBarContainer.Visible = visible;
        }
        OnVisibilityChanged?.Invoke(visible);
    }
    
    public void SetCustomColor(Color color)
    {
        var fillMaterial = _healthBarFill?.GetActiveMaterial(0) as StandardMaterial3D;
        if (fillMaterial != null)
        {
            fillMaterial.AlbedoColor = color;
        }
    }
    
    public void SetTextColor(Color color)
    {
        if (_healthTextLabel != null)
        {
            // Note: Label3D doesn't have direct color setting, would need material override
        }
    }
    
    public bool IsHealthBarVisible()
    {
        return _healthBarVisible;
    }
    
    public int GetCurrentHealth()
    {
        return _currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return _maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;
    }
} 