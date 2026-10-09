using UnityEngine;

public class EngineModel : MonoBehaviour
{
    /// <summary>エンジンのトルクカーブ（横軸RPM、縦軸N·m）</summary>
    [SerializeField] private AnimationCurve _torqueCurve;
    [SerializeField] private float _rpm;
    /// <summary>慣性モーメント（kg·m²）</summary>
    [SerializeField] private float _momentOfInertia = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _throttle;
    /// <summary>発生トルク（トルクカーブとスロットル開度から得られる値）</summary>
    [SerializeField] private float _generatedTorque;
    /// <summary>出力トルク（発生トルクから内部抵抗などを加味した最終的な出力）</summary>
    [SerializeField] private float _outputTorque;
    /// <summary>角速度の変化（出力トルクを慣性で割って求められる値）</summary>
    [SerializeField] private float _deltaAnguVel;
    /// <summary>回転数の変化量</summary>
    [SerializeField] private float _deltaRPM;

    private void FixedUpdate()
    {
        // 発生トルクを計算（現在のRPMから得られるトルク×スロットル開度）
        _generatedTorque = _torqueCurve.Evaluate(_rpm) * _throttle;

        // RPMに合わせて内部抵抗を計算する

        // 発生トルクと内部抵抗から出力トルクを求める
        // 10月9日：今日はまだ内部抵抗を求めないので、発生トルクを直接代入
        _outputTorque = _generatedTorque;

        // 出力トルクを慣性で割り、1フレームの時間を掛けて、角速度の変化を求める
        _deltaAnguVel = _outputTorque / _momentOfInertia * Time.deltaTime;

        // 角速度の変化をRPMの変化に換算する
        _deltaRPM = (_deltaAnguVel * 60f) / (2 * Mathf.PI);

        // 前フレームのRPMの値と今フレームのRPMの変化分を足し合わせる
        _rpm += _deltaRPM;
        
    }
}
