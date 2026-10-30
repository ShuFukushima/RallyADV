using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    [SerializeField] private InputActionReference _throttleInput;

    [SerializeField] private InputActionReference _brakeInput;
    [SerializeField] private InputActionReference _sideBrakeInput;
    [SerializeField] private InputActionReference _steerInput;
    [SerializeField] private InputActionReference _shiftUpInput;
    [SerializeField] private InputActionReference _shiftDownInput;

    public float Throttle => _throttleInput.action.ReadValue<float>();
    public float Brake => _brakeInput.action.ReadValue<float>();
    public float Steer => _steerInput.action.ReadValue<float>();
    public bool SideBrake => _sideBrakeInput.action.IsPressed();
    public bool ShiftUpPressed => _shiftUpInput.action.WasPressedThisFrame();
    public bool ShiftDownPressed => _shiftDownInput.action.WasPressedThisFrame();

    private void OnEnable()
    {
        // 有効にする
        _throttleInput.action.Enable();
        _brakeInput.action.Enable();
        _sideBrakeInput.action.Enable();
        _steerInput.action.Enable();
        _shiftUpInput.action.Enable();
        _shiftDownInput.action.Enable();
    }

    private void OnDisable()
    {
        // 無効にする
        _throttleInput.action.Disable();
        _brakeInput.action.Disable();
        _sideBrakeInput.action.Disable();
        _steerInput.action.Disable();
        _shiftUpInput.action.Disable();
        _shiftDownInput.action.Disable();
    }
}
