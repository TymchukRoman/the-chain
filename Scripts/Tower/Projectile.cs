using Godot;

public partial class Projectile : Node3D
{
    public Enemy Target { get; set; }
    public int Damage { get; set; }
    public float Speed { get; set; }
    
    private bool _hasHit = false;
    
    public override void _Ready()
    {
        // Create visual representation
        var meshInstance = new MeshInstance3D();
        var sphereMesh = new SphereMesh();
        sphereMesh.Radius = 0.1f;
        sphereMesh.Height = 0.2f;
        
        var material = new StandardMaterial3D();
        material.AlbedoColor = new Color(1.0f, 1.0f, 0.0f); // Yellow projectile
        sphereMesh.Material = material;
        
        meshInstance.Mesh = sphereMesh;
        AddChild(meshInstance);
    }
    
    public override void _Process(double delta)
    {
        if (_hasHit || Target == null || !IsInstanceValid(Target)) 
        {
            QueueFree();
            return;
        }
        
        // Move toward target
        Vector3 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        GlobalPosition += direction * Speed * (float)delta;
        
        // Check if we hit the target
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 0.5f)
        {
            HitTarget();
        }
    }
    
    private void HitTarget()
    {
        if (_hasHit) return;
        
        _hasHit = true;
        Target.TakeDamage(Damage);
        QueueFree();
    }
} 