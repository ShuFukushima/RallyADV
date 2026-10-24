using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    /// <summary>個々の車軸の情報（左右駆動輪・ハンドル）</summary>
    public List<AxleInfo> _axleInfos;
    /// <summary>最大舵角</summary>
    [SerializeField] private float _maxSteeringAngle; // 適用可能な最大ハンドル角度
    [SerializeField] private float _maxSideBrakeTorque;    // サイドブレーキの値
    public float _maxBrakeTorque;
    public float _steeringInput;
    public float _brakeInput;
    public float _sideBrakeInput;

    /// <summary>駆動輪の平均回転数</summary>
    [SerializeField] private float _driveWheelRPMAve = 0;
    [SerializeField] private int _driveWheelCount = 0;

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject _centerOfMass = null;  // 重心を表すオブジェクト

    [Header("参照")]
    ///<summary>実際に車輪を駆動させるトルクを求める</summary>
    [SerializeField] private DriveTrainModel _driveTrain;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_centerOfMass == null)
        {
            Debug.LogError("重心用の空オブジェクトを設定してください！");
            return;
        }

        // 重心位置を重心用オブジェクトに合わせる
        _rb.centerOfMass = _centerOfMass.transform.localPosition;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ステアリング
        _steeringInput = 0f;
        if (keyboard.aKey.isPressed) _steeringInput -= 1f;
        if (keyboard.dKey.isPressed) _steeringInput += 1f;

        // ブレーキ
        _brakeInput = keyboard.shiftKey.isPressed ? 1f : 0f;

        // サイドブレーキ
        _sideBrakeInput = keyboard.spaceKey .isPressed ? 1f : 0f;
    }

    public void FixedUpdate()
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
        float allWheelTorque = _driveTrain.DriveTrainStep(_driveWheelRPMAve);

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
}
