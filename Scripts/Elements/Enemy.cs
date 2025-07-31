using Godot;
using System.Collections.Generic;

public partial class Enemy : CharacterBody3D
{
    [Export] public float Speed = 2.0f;
    [Export] public int MaxHealth = 100;
    [Export] public int DamageToCastle = 10;
    
    private int _currentHealth;
    private Vector3 _targetPosition;
    private bool _isDead = false;
    private Game _game;
    private NavigationAgent3D _navigationAgent;
    private float _stuckTimer = 0.0f;
    private Vector3 _lastPosition;
    private const float STUCK_THRESHOLD = 0.1f; // Distance threshold for stuck detection
    private const float STUCK_TIME = 2.0f; // Time before considering stuck
    
    // Health bar components
    private Node3D _healthBarContainer;
    private MeshInstance3D _healthBarBackground;
    private MeshInstance3D _healthBarFill;
    private Label3D _healthTextLabel;
    private bool _healthBarVisible = false;
    
    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Root");
        
        // Find castle position (based on the actual castle position in the scene)
        _targetPosition = new Vector3(0.3f, 0, 6.6f); // Castle position from scene transform
        _lastPosition = GlobalPosition;
        
        SetupNavigationAgent();
        SetupHealthBar();
        
    }
    
    public void InitializeHealth()
    {
        _currentHealth = MaxHealth;
        UpdateHealthBar();
    }
    
    private void SetupHealthBar()
    {
        // Create health bar container
        _healthBarContainer = new Node3D();
        AddChild(_healthBarContainer);
        
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
        _healthTextLabel = new Label3D();
        _healthTextLabel.Text = _currentHealth + "/" + MaxHealth;
        _healthTextLabel.FontSize = 24;
        _healthTextLabel.PixelSize = 0.02f;
        _healthTextLabel.Position = new Vector3(0, 0.3f, 0.03f); // Above the health bar
        _healthTextLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _healthTextLabel.VerticalAlignment = VerticalAlignment.Center;
        _healthBarContainer.AddChild(_healthTextLabel);
        
        // Position health bar higher above enemy
        _healthBarContainer.Position = new Vector3(0, 1.5f, 0); // Higher position
        
        // Make health bar always face camera (using Sprite3D instead)
        var billboardSprite = new Sprite3D();
        billboardSprite.PixelSize = 0.01f;
        billboardSprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        billboardSprite.Visible = false; // We'll use this for billboard effect
        _healthBarContainer.AddChild(billboardSprite);
        
        // Initially hide health bar
        _healthBarContainer.Visible = false;
    }
    
    private void UpdateHealthBar()
    {
        if (_healthBarContainer == null) return;
        
        float healthPercentage = (float)_currentHealth / MaxHealth;
        
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
            _healthTextLabel.Text = _currentHealth + "/" + MaxHealth;
        }
        
        // Show health bar when damaged
        if (!_healthBarVisible && _currentHealth < MaxHealth)
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
        
        // Make health bar face the camera
        _healthBarContainer.LookAt(camera.GlobalPosition);
        _healthBarContainer.RotateY(Mathf.Pi); // Flip to face camera properly
    }
    
    private void SetupNavigationAgent()
    {
        // Create and configure NavigationAgent3D
        _navigationAgent = new NavigationAgent3D();
        AddChild(_navigationAgent);
        
        // Configure the navigation agent
        _navigationAgent.TargetPosition = _targetPosition;
        _navigationAgent.MaxSpeed = Speed;
        _navigationAgent.PathMaxDistance = 1.0f;
        _navigationAgent.PathDesiredDistance = 0.5f;
        
        // Connect signals
        _navigationAgent.TargetReached += OnTargetReached;
        _navigationAgent.NavigationFinished += OnNavigationFinished;
    }
    
    public override void _Process(double delta)
    {
        if (_isDead) return;
        
        // Check if enemy is stuck
        CheckIfStuck((float)delta);
        
        // Update navigation target if needed
        if (_navigationAgent.TargetPosition != _targetPosition)
        {
            _navigationAgent.TargetPosition = _targetPosition;
        }
        
        MoveAlongPath((float)delta);
        
        // Make health bar always face camera
        UpdateHealthBarRotation();
    }
    
    private void CheckIfStuck(float delta)
    {
        float distanceMoved = GlobalPosition.DistanceTo(_lastPosition);
        
        if (distanceMoved < STUCK_THRESHOLD)
        {
            _stuckTimer += delta;
            if (_stuckTimer >= STUCK_TIME)
            {
                ForcePathRecalculation();
                _stuckTimer = 0.0f;
            }
        }
        else
        {
            _stuckTimer = 0.0f;
        }
        
        _lastPosition = GlobalPosition;
    }
    
    private void ForcePathRecalculation()
    {
        // Force navigation agent to recalculate path
        _navigationAgent.TargetPosition = _targetPosition;
        _navigationAgent.GetNextPathPosition();
    }
    
    private void MoveAlongPath(float delta)
    {
        if (_navigationAgent.IsNavigationFinished())
        {
            // We've reached the target
            ReachCastle();
            return;
        }
        
        // Check if we're close enough to the castle
        float distanceToCastle = GlobalPosition.DistanceTo(_targetPosition);
        if (distanceToCastle < 1.0f) // If within 1 unit of castle
        {
            ReachCastle();
            return;
        }
        
        // Get the next position to move towards
        Vector3 nextPosition = _navigationAgent.GetNextPathPosition();
        Vector3 direction = (nextPosition - GlobalPosition).Normalized();
        
        // Use CharacterBody3D's move_and_slide for proper collision handling
        Velocity = direction * Speed;
        MoveAndSlide();
    }
    
    private void OnTargetReached()
    {
        ReachCastle();
    }
    
    private void OnNavigationFinished()
    {
        ReachCastle();
    }
    
    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        
        _currentHealth -= damage;
        
        // Ensure health doesn't go below 0
        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }
        
        
        // Update health bar
        UpdateHealthBar();
        
        // Check if enemy should die (health <= 0)
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (_isDead) return; // Prevent multiple death calls
        
        _isDead = true;
        _game.AddRP(2); // +10 RP for killing enemy
        QueueFree();
    }
    
    private void ReachCastle()
    {
        _isDead = true;
        _game.AddRP(-20); // -20 RP for enemy reaching castle
        QueueFree();
    }
} 