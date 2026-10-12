using UnityEngine;

public class EngineModel : MonoBehaviour
{
    /// <summary>エンジンのトルクカーブ（横軸RPM、縦軸N·m）これは公称値のため、すでに内部抵抗分がひかれた後の値になる点に注意</summary>
    [SerializeField] private AnimationCurve _torqueCurve;
    [SerializeField] private float _rpm;
    /// <summary>アイドル回転数</summary>
    [SerializeField] private float _idleRPM = 900f;
    [SerializeField] private float _revLimitRPM = 7000f;
    /// <summary>慣性モーメント（kg·m²）</summary>
    [SerializeField, Range(0.001f, 1f)] private float _momentOfInertia = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _throttle;
    [SerializeField] private float _idleThrottle = 0.2f;
    [SerializeField] private float _appliedThrottle;
    /// <summary>発生トルク（トルクカーブとスロットル開度から得られる値）</summary>
    [SerializeField] private float _generatedTorque;
    /// <summary>出力トルク（発生トルクから内部抵抗などを加味した最終的な出力）</summary>
    [SerializeField] private float _outputTorque;
    /// <summary>角速度の変化（出力トルクを慣性で割って求められる値）</summary>
    [SerializeField] private float _deltaAnguVel;
    /// <summary>回転数の変化量</summary>
    [SerializeField] private float _deltaRPM;
    /// <summary>最終的な内部抵抗（N・m）</summary>
    [SerializeField] private float _internalResistance;
    /// <summary>基礎内部抵抗</summary>
    [SerializeField] private float _baseInternalResistance = 5f;
    /// <summary>内部抵抗の増加係数（N·m/RPM）</summary>
    [SerializeField] private float _coefficientInternalResistance = 0.001f;


    [Header("テスト用変数群")]
    [SerializeField] private float _deltaAngTimer = 0f;
    [SerializeField] private float _changeMinRPM = 1000f;
    [SerializeField] private float _changeMaxRPM = 6000f;

    private void FixedUpdate()
    {
        // RPMに合わせて内部抵抗を計算する（基礎内部抵抗 + RPM * 係数
        _internalResistance = _baseInternalResistance + (_rpm * _coefficientInternalResistance);

        // 内部抵抗はRPMが0以下の時は0
        if (_rpm <= 0f)
        {
            _internalResistance = 0f;
        }

        // 使用するスロットルを決定する
        // 現在のRPMがアイドルRPMよりも低く、かつ、プレイヤーのスロットル入力がアイドルスロットルより低いなら
        if(_rpm <= _idleRPM && _throttle <= _idleThrottle)
        {
            _appliedThrottle = _idleThrottle;
        }
        else if(_rpm >= _revLimitRPM)
        {
            // 現在のRPMがレブリミット以上であればスロットルを0にする
            _appliedThrottle = 0f;
        }
        else
        {
            _appliedThrottle = _throttle;
        }

        // 発生トルクを計算（(現在のRPMから得られるトルク + 公称値に合わせるための内部抵抗分加算)×スロットル開度）
        _generatedTorque = (_torqueCurve.Evaluate(_rpm) + _internalResistance) * _appliedThrottle;

        // 発生トルクと内部抵抗から出力トルクを求める
        _outputTorque = _generatedTorque - _internalResistance;

        // 出力トルクを慣性で割り、1フレームの時間を掛けて、角速度の変化を求める
        _deltaAnguVel = _outputTorque / _momentOfInertia * Time.deltaTime;

        // 角速度の変化をRPMの変化に換算する
        _deltaRPM = (_deltaAnguVel * 60f) / (2 * Mathf.PI);

        // 前フレームのRPMの値と今フレームのRPMの変化分を足し合わせる
        _rpm += _deltaRPM;

        // RPMを0未満にしない
        if(_rpm < 0f )
        {
            _rpm = 0f;
        }


        // RPMの変化時間を見るための処理
        if(_rpm < _changeMaxRPM && _rpm > _changeMinRPM)
        {
            _deltaAngTimer += Time.deltaTime;
        }
        
    }
}
