using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; //このホイールがエンジンにアタッチされているかどうか
    public bool steering; // このホイールがハンドルの角度を反映しているかどうか
}

public class PCarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // 個々の車軸の情報
    public float _maxMotorTorque; //ホイールに適用可能な最大トルク
    public float _maxSteeringAngle; // 適用可能な最大ハンドル角度
    public float _maxBrakeTorque;
    public float _throttleInput;
    public float _steeringInput;
    public float _brakeInput;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // スロットル
        if (keyboard.wKey.isPressed)
        {
            _throttleInput = 1f;
        }
        else if (keyboard.sKey.isPressed)
        {
            _throttleInput = -1f;
        }
        else
        {
            _throttleInput = 0f;
        }

        // ステアリング
        _steeringInput = 0f;
        if (keyboard.aKey.isPressed) _steeringInput -= 1f;
        if (keyboard.dKey.isPressed) _steeringInput += 1f;

        // ブレーキ
        _brakeInput = keyboard.shiftKey.isPressed ? 1f : 0f;
    }

    public void FixedUpdate()
    {
        float motor = _maxMotorTorque * _throttleInput;
        float steering = _maxSteeringAngle * _steeringInput;
        float brake = _maxBrakeTorque * _brakeInput;

        foreach (AxleInfo axleInfo in axleInfos)
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
            
            axleInfo.leftWheel.brakeTorque = brake;
            axleInfo.rightWheel.brakeTorque = brake;
        }
    }

}
