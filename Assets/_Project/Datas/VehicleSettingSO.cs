using UnityEngine;

[CreateAssetMenu(fileName = "VehicleSetting.cs", menuName = "ScriptableObjects/VehicleSetting")]
public class VehicleSettingSO : ScriptableObject
{
    [Header("車両基礎設定")]
    [SerializeField] private float _mass;
    [SerializeField] private Vector3 _centerOfMass;
    [SerializeField] private float _maxSteeringAngle;
    [SerializeField] private float _maxBrakeTorque;
    [SerializeField] private float _maxSideBrakeTorque;
    [SerializeField] private bool[] _axleMotor;
    [SerializeField] private bool[] _axleSteering;

    [Header("WheelCollider設定")]
    [SerializeField] private float _wheelRadius;
    [SerializeField] private float _wheelDampingRate;
    [SerializeField] private float _suspensionDistance;

    [Header("サスペンション（Suspension Spring）")]
    [SerializeField] private float _spring;
    [SerializeField] private float _damper;
    [SerializeField] private float _targetPosition;

    [Header("前後方向の摩擦（Forward Friction）")]
    [SerializeField] private float _forwardExtremumSlip;
    [SerializeField] private float _forwardExtremumValue;
    [SerializeField] private float _forwardAsymptoteSlip;
    [SerializeField] private float _forwardAsymptoteValue;
    [SerializeField] private float _forwardStiffness;

    [Header("横方向の摩擦（Sideways Friction）")]
    [SerializeField] private float _sidewaysExtremumSlip;
    [SerializeField] private float _sidewaysExtremumValue;
    [SerializeField] private float _sidewaysAsymptoteSlip;
    [SerializeField] private float _sidewaysAsymptoteValue;
    [SerializeField] private float _sidewaysStiffness;

    [Header("エンジン設定")]
    [SerializeField] private AnimationCurve _torqueCurve;
    [SerializeField] private float _idleRPM;
    [SerializeField] private float _idleThrottle;
    [SerializeField] private float _revLimitRPM;
    [SerializeField, Range(0.001f, 1f)] private float _momentOfInertia;
    [SerializeField] private float _baseInternalResistance;
    [SerializeField] private float _coefficientInternalResistance;

    [Header("ドライブトレイン設定")]
    [SerializeField] private float[] _gearsRatio;
    [SerializeField] private float _finalRatio;
    [SerializeField] private float _transmissionEfficiency;
    [SerializeField] private float _clutchCoefficient;
    [SerializeField] private float _maxClutchTorque;
    [SerializeField] private float _clutchEngageStartRPM;
    [SerializeField] private float _clutchFullEngageRPM;
    [SerializeField] private float _atShiftUpRPM;
    [SerializeField] private float _atShiftDownRPM;
    [SerializeField] private float _shiftWaitTime;

    // ===== プロパティ =====

    // 車両基礎設定
    public float Mass => _mass;
    public Vector3 CenterOfMass => _centerOfMass;
    public float MaxSteeringAngle => _maxSteeringAngle;
    public float MaxBrakeTorque => _maxBrakeTorque;
    public float MaxSideBrakeTorque => _maxSideBrakeTorque;
    public bool[] AxleMotor => _axleMotor;
    public bool[] AxleSteering => _axleSteering;

    // WheelCollider設定
    public float WheelRadius => _wheelRadius;
    public float WheelDampingRate => _wheelDampingRate;
    public float SuspensionDistance => _suspensionDistance;
    public float Spring => _spring;
    public float Damper => _damper;
    public float TargetPosition => _targetPosition;
    public float ForwardExtremumSlip => _forwardExtremumSlip;
    public float ForwardExtremumValue => _forwardExtremumValue;
    public float ForwardAsymptoteSlip => _forwardAsymptoteSlip;
    public float ForwardAsymptoteValue => _forwardAsymptoteValue;
    public float ForwardStiffness => _forwardStiffness;
    public float SidewaysExtremumSlip => _sidewaysExtremumSlip;
    public float SidewaysExtremumValue => _sidewaysExtremumValue;
    public float SidewaysAsymptoteSlip => _sidewaysAsymptoteSlip;
    public float SidewaysAsymptoteValue => _sidewaysAsymptoteValue;
    public float SidewaysStiffness => _sidewaysStiffness;

    // エンジン設定
    public AnimationCurve TorqueCurve => _torqueCurve;
    public float IdleRPM => _idleRPM;
    public float IdleThrottle => _idleThrottle;
    public float RevLimitRPM => _revLimitRPM;
    public float MomentOfInertia => _momentOfInertia;
    public float BaseInternalResistance => _baseInternalResistance;
    public float CoefficientInternalResistance => _coefficientInternalResistance;

    // ドライブトレイン設定
    public float[] GearsRatio => _gearsRatio;
    public float FinalRatio => _finalRatio;
    public float TransmissionEfficiency => _transmissionEfficiency;
    public float ClutchCoefficient => _clutchCoefficient;
    public float MaxClutchTorque => _maxClutchTorque;
    public float ClutchEngageStartRPM => _clutchEngageStartRPM;
    public float ClutchFullEngageRPM => _clutchFullEngageRPM;
    public float AtShiftUpRPM => _atShiftUpRPM;
    public float AtShiftDownRPM => _atShiftDownRPM;
    public float ShiftWaitTime => _shiftWaitTime;
}
