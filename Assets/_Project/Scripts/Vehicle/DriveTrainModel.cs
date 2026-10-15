using UnityEngine;
using UnityEngine.InputSystem;

// クラッチ・ギア・ファイナル・タイヤの回転までを表すモデル
public class DriveTrainModel : MonoBehaviour
{
    [Tooltip("車輪の回転数（テスト用）")]
    [SerializeField] private float _wheelRPM;
    [Tooltip("ギアを格納する配列（0が後進、1～が前進用）")]
    [SerializeField] private float[] _gearsRatio;
    [Tooltip("現在のギア")]
    [SerializeField] private int _nowGear;
    [Tooltip("ファイナル比")]
    [SerializeField] private float _finalRatio;
    [Tooltip("車輪の回転数から逆算される、エンジン側相当のRPM（テスト・表示用）")]
    [SerializeField] private float _engineRPMFromWheel;
    [Tooltip("エンジン側トルク（N・m テスト用手入力 現在は未使用）")]
    [SerializeField] private float _testEngineTorque;
    [Tooltip("動力の伝達効率（0～1）")]
    [SerializeField, Range(0f, 1f)] private float _transmissionEfficiency = 1f;
    [Tooltip("車輪側トルク（表示用）")]
    [SerializeField] private float _wheelTorque;
    [Tooltip("エンジンRPM（実際のエンジンから。現在はテスト用）")]
    [SerializeField] private float _engineRPM;
    [Tooltip("クラッチの接続率（0で切断、1で完全につながる）")]
    [SerializeField, Range(0f, 1f)] private float _clutchConnectRate = 0f;
    [Tooltip("クラッチの係数（N・m/RPM）")]
    [SerializeField] private float _clutchCoefficient = 0.05f;
    [Tooltip("最大クラッチトルク（N・m 仮で100）")]
    [SerializeField] private float _maxClutchTorque = 100f;
    [Tooltip("回転差（表示用）")]
    [SerializeField] private float _rotateDiff;
    [Tooltip("クラッチトルク（表示用 プラスならエンジンが車輪を押し、マイナスなら車輪がエンジンを回す）")]
    [SerializeField] private float _currentClutchTorque;
    [Tooltip("接地判定")]
    [SerializeField] private bool _isGrounded = false;
    [Tooltip("車輪側の慣性（駆動輪分の値 kg・m²）")]
    [SerializeField] private float _wheelInertia = 5f;
    [Tooltip("車重")]
    [SerializeField] private float _vehicleMass = 700f;
    [Tooltip("車輪の半径")]
    [SerializeField] private float _wheelRadius = 0.273f;
    [Tooltip("エンジン側の慣性（EngineModelの採用値と同値）")]
    [SerializeField] private float _momentOfInertia = 0.1f;
    [Tooltip("車輪側の慣性（表示用）")]
    [SerializeField] private float _wheelInertiaVis;

    private void FixedUpdate()
    {
        // エンジン側相当のRPM
        // エンジン側相当RPM = 車輪のRPM * ギア比 * ファイナル比
        _engineRPMFromWheel = _wheelRPM * _gearsRatio[_nowGear] * _finalRatio;

        // 回転差
        _rotateDiff = _engineRPM - _engineRPMFromWheel;

        // クラッチトルク
        // クラッチトルク = 回転差 * 係数
        _currentClutchTorque = _rotateDiff * _clutchCoefficient;
        // 範囲内に収め、接続率をかける
        _currentClutchTorque = Mathf.Clamp(_currentClutchTorque, -_maxClutchTorque, _maxClutchTorque) * _clutchConnectRate;

        // 車輪側のトルク（エンジン側トルクの手入力代わりにクラッチトルクを使用）
        // 車輪側トルク = クラッチトルク * ギア比 * ファイナル比 * 伝達効率
        _wheelTorque = _currentClutchTorque * _gearsRatio[_nowGear] * _finalRatio * _transmissionEfficiency;

        // 現在の車輪側の慣性を求める
        float currentWheelInertia;
        if(_isGrounded)
        {
            // 接地している：車輪側慣性 = 車輪側の慣性 + 車重 * ホイール半径^2
            currentWheelInertia = _wheelInertia + _vehicleMass * _wheelRadius * _wheelRadius;
        }
        else
        {
            // 接地していない：車輪側の慣性 = 車輪側の慣性のまま
            currentWheelInertia = _wheelInertia;
        }
        _wheelInertiaVis = currentWheelInertia;

        // エンジン側の角速度の変化（RPM換算）
        float deltaEngineAngVel = -_currentClutchTorque / _momentOfInertia * Time.deltaTime;
        // 角速度の変化をRPMに換算してエンジン側RPMに足し合わせる
        _engineRPM += (deltaEngineAngVel * 60f) / (2 * Mathf.PI);

        // 車輪の角速度の変化
        float deltaWheelAngVel = _wheelTorque / currentWheelInertia * Time.deltaTime;
        _wheelRPM += (deltaWheelAngVel * 60f) / (2 * Mathf.PI);
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
