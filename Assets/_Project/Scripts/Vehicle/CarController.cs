using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    /// <summary>個々の車軸の情報（左右駆動輪・ハンドル）</summary>
    [SerializeField] private List<AxleInfo> _axleInfos;
    /// <summary>最大舵角</summary>
    [SerializeField] private float _maxSteeringAngle; // 適用可能な最大ハンドル角度
    [SerializeField] private float _maxSideBrakeTorque;    // サイドブレーキの値
    [SerializeField] private float _maxBrakeTorque;
    [SerializeField] private float _throttleInput;
    [SerializeField] private float _steeringInput;
    [SerializeField] private float _brakeInput;
    [SerializeField] private float _sideBrakeInput;

    /// <summary>駆動輪の平均回転数</summary>
    [SerializeField] private float _driveWheelRPMAve = 0;
    [SerializeField] private int _driveWheelCount = 0;

    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float _speedKmh = 0;
    private WheelCollider[] _wheels = new WheelCollider[4];

    [Header("プロパティ")]
    /// <summary>速度のプロパティ</summary>
    public float SpeedKmh => _speedKmh;

    [Header("参照")]
    ///<summary>実際に車輪を駆動させるトルクを求める</summary>
    [SerializeField] private DriveTrainModel _driveTrain;
    /// <summary>車両個別の設定</summary>
    [SerializeField] private VehicleSettingSO _setting = null;
    private VehicleInput _input;

    private void Awake()
    {
        // 設定を写す
        if( _setting == null)
        {
            Debug.LogError("CarControllerにVehicle Setting SOが設定されていません！");
            return;
        }
        // Rigidbody
        _rb = GetComponent<Rigidbody>();
        _rb.mass = _setting.Mass;
        _rb.centerOfMass = _setting.CenterOfMass;

        // CarController
        _maxSteeringAngle = _setting.MaxSteeringAngle;
        _maxBrakeTorque = _setting.MaxBrakeTorque;
        _maxSideBrakeTorque = _setting.MaxSideBrakeTorque;
        _axleInfos[0].motor = _setting.AxleMotor[0];
        _axleInfos[1].motor = _setting.AxleMotor[1];
        _axleInfos[0].steering = _setting.AxleSteering[0];
        _axleInfos[1].steering = _setting.AxleSteering[1];

        // WheelCollider の箱を先に組み立てる
        JointSpring suspensionSpring = new JointSpring();
        suspensionSpring.spring = _setting.Spring;
        suspensionSpring.damper = _setting.Damper;
        suspensionSpring.targetPosition = _setting.TargetPosition;

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = _setting.ForwardExtremumSlip;
        forwardFriction.extremumValue = _setting.ForwardExtremumValue;
        forwardFriction.asymptoteSlip = _setting.ForwardAsymptoteSlip;
        forwardFriction.asymptoteValue = _setting.ForwardAsymptoteValue;
        forwardFriction.stiffness = _setting.ForwardStiffness;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = _setting.SidewaysExtremumSlip;
        sidewaysFriction.extremumValue = _setting.SidewaysExtremumValue;
        sidewaysFriction.asymptoteSlip = _setting.SidewaysAsymptoteSlip;
        sidewaysFriction.asymptoteValue = _setting.SidewaysAsymptoteValue;
        sidewaysFriction.stiffness = _setting.SidewaysStiffness;

        // WheelCollider
        foreach (AxleInfo axleInfo in _axleInfos)
        {
            WheelCollider[] wheels = { axleInfo.leftWheel, axleInfo.rightWheel };
            foreach (WheelCollider wheel in wheels)
            {
                wheel.radius = _setting.WheelRadius;
                wheel.wheelDampingRate = _setting.WheelDampingRate;
                wheel.suspensionDistance = _setting.SuspensionDistance;
                wheel.suspensionSpring = suspensionSpring;
                wheel.forwardFriction = forwardFriction;
                wheel.sidewaysFriction = sidewaysFriction;
            }

        }

        // DriveTrainModelのスクリプタブルオブジェクトを設定する
        _driveTrain.SetSetting(_setting);

        // つけ忘れ防止
        _input = GetComponent<VehicleInput>();
        if( _input == null)
        {
            Debug.LogError("VehicleInputを設定してください。");
            return;
        }

    }

    private void Start()
    {

        // 情報を各輪ごとに読み取る
        _wheels[0] = _axleInfos[0].leftWheel;
        _wheels[1] = _axleInfos[0].rightWheel;
        _wheels[2] = _axleInfos[1].leftWheel;
        _wheels[3] = _axleInfos[1].rightWheel;
        
    }

    private void Update()
    {
        // スロットル
        if (_input.Throttle > 0)
        {
            _throttleInput += 1f * Time.deltaTime;
        }
        else
        {
            _throttleInput = 0f;
        }
        if (_throttleInput >= 1.0f)
        {
            _throttleInput = 1.0f;
        }

        // ステアリング
        if(_input.Steer > 0)
        {
            _steeringInput += 1f * Time.deltaTime;
        }
        else if(_input.Steer < 0)
        {
            _steeringInput -= 1f * Time.deltaTime;
        }
        else
        {
            _steeringInput = 0f;
        }
        _steeringInput = Mathf.Clamp(_steeringInput, -1f, 1f);
        
        // ブレーキ
        _brakeInput = _input.Brake;

        // サイドブレーキ
        _sideBrakeInput = _input.SideBrake ? 1f : 0f;

        // 変速処理
        if (_input.ShiftUpPressed)
        {
            _driveTrain.ShiftUp();
        }
        if(_input.ShiftDownPressed)
        {
            _driveTrain.ShiftDown();
        }

        // 速度取得
        _speedKmh = _rb.linearVelocity.magnitude * 3.6f;
    }

    private void FixedUpdate()
    {


        // まず駆動輪かどうか、駆動輪の回転平均を求め、ドライブトレインから車輪側のトルクを算出する
        _driveWheelRPMAve = 0f;
        _driveWheelCount = 0;
        foreach (AxleInfo axleInfo in _axleInfos)
        {
            if (axleInfo.motor)
            {
                _driveWheelRPMAve += axleInfo.leftWheel.rpm;
                _driveWheelRPMAve += axleInfo.rightWheel.rpm;

                _driveWheelCount += 2;
            }

        }

        // 駆動輪の平均回転数を算出
        _driveWheelRPMAve = _driveWheelRPMAve / (float)_driveWheelCount;

        // 駆動輪の総トルクを算出（ドライブトレイン→エンジン）
        float allWheelTorque = _driveTrain.DriveTrainStep(_driveWheelRPMAve, _throttleInput);

        // 駆動輪1つのトルクは総トルク÷駆動輪数
        float motor = allWheelTorque / (float)_driveWheelCount;
        float steering = _maxSteeringAngle * _steeringInput;
        float brake = _maxBrakeTorque * _brakeInput;
        float sideBrake = _maxSideBrakeTorque * _sideBrakeInput;

        // トルク・ステアリング・ブレーキを適用する
        foreach (AxleInfo axleInfo in _axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            if(axleInfo.rearWheel)
            {
                // 後輪ならサイドブレーキのトルクを加える（押してなければ0）
                axleInfo.leftWheel.brakeTorque = brake + sideBrake;
                axleInfo.rightWheel.brakeTorque = brake + sideBrake;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = brake;
                axleInfo.rightWheel.brakeTorque = brake;
            }


        }
    }

    /// <summary>
    /// 指定した車輪が接地しているかを返す
    /// </summary>
    /// <param name="index"></param>
    public bool IsWheelGrounded(int index)
    {
        return _wheels[index].isGrounded;
    }

    /// <summary>
    /// 指定した車輪の前後スリップ
    /// </summary>
    /// <param name="index"></param>
    public float GetForwardSlip(int index)
    {
        // 接地情報を格納する構造体
        WheelHit hit;

        // ホイールが地面に接地している場合のみスリップ値を取得
        if (_wheels[index].GetGroundHit(out hit))
        {
            // 縦スリップ
            return hit.forwardSlip;

        }
        else
        {
            return 0f;
        }
    }

    /// <summary>
    /// 指定した車輪の左右スリップ
    /// </summary>
    /// <param name="index"></param>
    public float GetSidewaysSlip(int index)
    {
        // 接地情報を格納する構造体
        WheelHit hit;

        // ホイールが地面に接地している場合のみスリップ値を取得
        if (_wheels[index].GetGroundHit(out hit))
        {
            // 横スリップ
            return hit.sidewaysSlip;

        }
        else
        {
            return 0f;
        }
    }
}
