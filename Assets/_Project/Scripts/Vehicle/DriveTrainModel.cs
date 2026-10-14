using System;
using UnityEngine;
using UnityEngine.InputSystem;

// クラッチ・ギア・ファイナル・タイヤの回転までを表すモデル
public class DriveTrainModel : MonoBehaviour
{
    [Tooltip("手入力する車輪の回転数（テスト用）")]
    [SerializeField] private float _wheelRPM;
    [Tooltip("ギアを格納する配列（0が後進、1～が前進用）")]
    [SerializeField] private float[] _gearsRatio;
    [Tooltip("現在のギア")]
    [SerializeField] private int _nowGear;
    [Tooltip("ファイナル比")]
    [SerializeField] private float _finalRatio;
    [Tooltip("車輪の回転数から逆算される、エンジン側相当のRPM（テスト・表示用）")]
    [SerializeField] private float _engineRPMFromWheel;
    [Tooltip("エンジン側トルク（N・m テスト用手入力）")]
    [SerializeField] private float _testEngineTorque;
    [Tooltip("動力の伝達効率（0～1）")]
    [SerializeField, Range(0f, 1f)] private float _transmissionEfficiency = 1f;
    [Tooltip("車輪側トルク（表示用）")]
    [SerializeField] private float _wheelTorque;

    private void FixedUpdate()
    {
        // エンジン側相当のRPM
        // エンジン側相当RPM = 車輪のRPM * ギア比 * ファイナル比
        _engineRPMFromWheel = _wheelRPM * _gearsRatio[_nowGear] * _finalRatio;

        // 車輪側のトルク
        // 車輪側トルク = エンジン側トルク * ギア比 * ファイナル比 * 伝達効率
        _wheelTorque = _testEngineTorque * _gearsRatio[_nowGear] * _finalRatio * _transmissionEfficiency;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        // ギアの切り替え
        if (keyboard.aKey.wasPressedThisFrame && _nowGear > 0)
        {
            _nowGear--;
        }
        else if(keyboard.dKey.wasPressedThisFrame && _nowGear < _gearsRatio.Length - 1)
        {
            _nowGear++;
        }
    }

}
