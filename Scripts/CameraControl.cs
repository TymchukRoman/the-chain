using Godot;
namespace Game;

public partial class CameraControl : Camera3D
{
    private Vector3 _velocity = Vector3.Zero;
    private float _moveSpeed = 700f;
    private float _zoomSpeed = 10f;
    private float _targetFov;
    private float _fovLerpSpeed = 5f;

    private bool _isPanning = false;
    private Vector2 _lastMousePosition = Vector2.Zero;
    private float _panSpeed = 0.5f;
    private readonly Vector3 right = new Vector3(1, 0, 0);   // World X
    private readonly Vector3 forward = new Vector3(0, 0, 1); // World Z
    private Vector3 _minBounds = new Vector3(-500, 0, -500);
    private Vector3 _maxBounds = new Vector3(500, 500, 500);

    public override void _Ready()
    {
        _targetFov = Fov;
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        return new Vector3(
            Mathf.Clamp(pos.X, _minBounds.X, _maxBounds.X),
            Mathf.Clamp(pos.Y, _minBounds.Y, _maxBounds.Y),
            Mathf.Clamp(pos.Z, _minBounds.Z, _maxBounds.Z)
        );
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
            {
                _isPanning = mouseButtonEvent.Pressed;
                if (_isPanning)
                    _lastMousePosition = mouseButtonEvent.Position;
            }

            // Scroll zoom
            if (mouseButtonEvent.Pressed)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
                    _targetFov -= _zoomSpeed;
                else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
                    _targetFov += _zoomSpeed;

                _targetFov = Mathf.Clamp(_targetFov, 30f, 80f);
            }
        }

        if (_isPanning && @event is InputEventMouseMotion mouseMotion)
        {
            Vector2 mouseDelta = mouseMotion.Relative;

            Vector3 panOffset = new Vector3(-mouseDelta.X, mouseDelta.Y, 0) * _panSpeed;
            Translate(panOffset);

        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 direction = Vector3.Zero;

        if (Input.IsActionPressed("Forward"))
            direction.Y += 1;
        if (Input.IsActionPressed("Back"))
            direction.Y -= 1;
        if (Input.IsActionPressed("Left"))
            direction.X -= 1;
        if (Input.IsActionPressed("Right"))
            direction.X += 1;

        direction = direction.Normalized();
        _velocity = direction * _moveSpeed;
        TranslateObjectLocal(_velocity * (float)delta);

        GlobalPosition = ClampPosition(GlobalPosition);

        // Smooth zoom
        Fov = Mathf.Lerp(Fov, _targetFov, _fovLerpSpeed * (float)delta);
    }
}
