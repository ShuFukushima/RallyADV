using UnityEngine;
using UnityEngine.InputSystem;

// クラッチ・ギア・ファイナル・タイヤの回転までを表すモデル
public class DriveTrainModel : MonoBehaviour
{
    [Tooltip("車輪の回転数（CarControllerから取得）")]
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
    [Tooltip("エンジンRPM（実際のエンジンから読んだ値を表示）")]
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
    [Tooltip("クラッチ接続時のエンジンRPM")]
    [SerializeField] private float _clutchEngageStartRPM = 900f;
    [Tooltip("クラッチフル接続のエンジンRPM")]

    [SerializeField] private float _clutchFullEngageRPM = 5000f;
    [Tooltip("ATシフトアップ時の回転数")]
    [SerializeField] private float _atShiftUpRPM = 6000f;
    [Tooltip("シフトチェンジ直後の待ち時間")]
    [SerializeField] private float _shiftWaitTime = 0.5f;
    [Tooltip("前のシフト操作から経った時間")]
    [SerializeField] private float _onShiftTime = 0.5f;

    [Header("参照")]
    [SerializeField] private EngineModel _engine;

    /// <summary>
    /// ドライブトレインの1フレームのモデル 順番を明確化するため、CarController から呼び出す
    /// </summary>
    /// <param name="loadWheelRPM"></param>
    public float DriveTrainStep(float loadWheelRPM)
    {
        // エンジンのRPMを取得
        _engineRPM = _engine.EngineRPM;

        // ギアの切り替え（オートマ）
        // 切り替え後、必ずRPM等の更新を行うため、こっちで行う
        _onShiftTime += Time.deltaTime;
        // 指定回転数以上 かつ 最大ギアではない　かつ　前進ギア　かつ　前のシフト操作から一定時間たっていれば（後進中に回転数が上がって1速にならないように）シフトアップ
        if ((_engineRPM >= _atShiftUpRPM) && (_nowGear < _gearsRatio.Length - 1) && (_nowGear != 0) && (_onShiftTime >= _shiftWaitTime))
        {
            _nowGear++;
            // 経過時間を0に戻す
            _onShiftTime = 0f;
        }

        // ホイールRPMを引数から代入
        _wheelRPM = loadWheelRPM;

        // エンジン側相当のRPM
        // エンジン側相当RPM = 車輪のRPM * ギア比 * ファイナル比
        _engineRPMFromWheel = _wheelRPM * _gearsRatio[_nowGear] * _finalRatio;

        // 回転差
        _rotateDiff = _engineRPM - _engineRPMFromWheel;

        // クラッチトルク
        // クラッチトルク = 回転差 * 係数
        _currentClutchTorque = _rotateDiff * _clutchCoefficient;

        // 接続率をエンジンRPMで変更する
        float clutchConnectRate = Mathf.InverseLerp(_clutchEngageStartRPM, _clutchFullEngageRPM, _engine.EngineRPM);
        _clutchConnectRate = clutchConnectRate;

        // 範囲内に収め、接続率をかける
        _currentClutchTorque = Mathf.Clamp(_currentClutchTorque, -_maxClutchTorque, _maxClutchTorque) * _clutchConnectRate;

        // エンジンの処理を呼び出す
        _engine.EngineStep(_currentClutchTorque);

        // 車輪側のトルク（エンジン側トルクの手入力代わりにクラッチトルクを使用）
        // 車輪側トルク = クラッチトルク * ギア比 * ファイナル比 * 伝達効率
        _wheelTorque = _currentClutchTorque * _gearsRatio[_nowGear] * _finalRatio * _transmissionEfficiency;

        // 駆動輪の総トルクを返す
        return _wheelTorque;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        // ギアの切り替え（マニュアル）
        if (keyboard.downArrowKey.wasPressedThisFrame && _nowGear > 0)
        {
            _nowGear--;
        }
        else if(keyboard.upArrowKey.wasPressedThisFrame && _nowGear < _gearsRatio.Length - 1)
        {
            _nowGear++;
        }


    }

}
