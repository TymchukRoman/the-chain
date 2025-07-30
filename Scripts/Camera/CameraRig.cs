using Godot;

public partial class CameraRig : Node3D
{
    [Export] public float MoveSpeed = 170f;
    [Export] public float RotateSpeed = 1f;
    [Export] public float ZoomSpeed = 2f;
    [Export] public float MinZoom = 5f;
    [Export] public float MaxZoom = 80f;

    private Node3D _pivot;
    private Camera3D _camera;

    public override void _Ready()
    {
        _pivot = GetNode<Node3D>("CameraPivot");
        _camera = _pivot.GetNode<Camera3D>("Camera3D");
    }

    public override void _Process(double delta)
    {
        HandleMovement((float)delta);
        HandleRotation((float)delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
                AdjustZoom(ZoomSpeed);
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
                AdjustZoom(-ZoomSpeed);
        }
    }

    private void AdjustZoom(float deltaZoom)
    {
        Vector3 camPos = _camera.Position;
        camPos.Y -= deltaZoom;
        camPos.Y = Mathf.Clamp(camPos.Y, MinZoom, MaxZoom);
        _camera.Position = camPos;
    }

    private void HandleMovement(float delta)
    {
        Vector2 input = Vector2.Zero;

        if (Input.IsActionPressed("move_forward")) input.Y += 1;
        if (Input.IsActionPressed("move_backward")) input.Y -= 1;
        if (Input.IsActionPressed("move_left")) input.X -= 1;
        if (Input.IsActionPressed("move_right")) input.X += 1;

        if (input == Vector2.Zero)
            return;

        input = input.Normalized();

        Vector3 forward = -_pivot.GlobalTransform.Basis.Z;
        forward.Y = 0;
        forward = forward.Normalized();

        Vector3 right = _pivot.GlobalTransform.Basis.X;
        right.Y = 0;
        right = right.Normalized();

        float zoomFactor = _camera.Position.Y / MaxZoom;
        zoomFactor = Mathf.Clamp(zoomFactor, 0.25f, 1f);

        float speed = MoveSpeed * zoomFactor;
        Vector3 direction = forward * input.Y + right * input.X;
        Translate(direction * speed * delta);
    }

    private void HandleRotation(float delta)
    {
        if (Input.IsActionPressed("rotate_left"))
        {
            _pivot.RotateY(Mathf.DegToRad(RotateSpeed));
        }
        if (Input.IsActionPressed("rotate_right"))
        {
            _pivot.RotateY(Mathf.DegToRad(-RotateSpeed));
        }
    }
}
