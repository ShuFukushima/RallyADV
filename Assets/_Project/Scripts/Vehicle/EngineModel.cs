using UnityEngine;

public class EngineModel : MonoBehaviour
{
    /// <summary>エンジンのトルクカーブ（横軸RPM、縦軸N·m）</summary>
    [SerializeField] private AnimationCurve _torqueCurve;
    [SerializeField] private float _rpm;
    [SerializeField, Range(0f, 1f)] private float _throttle;
    /// <summary>発生トルク（トルクカーブとスロットル開度から得られる値）</summary>
    [SerializeField] private float _generatedTorque;

    private void FixedUpdate()
    {
        // 発生トルクを計算（現在のRPMから得られるトルク×スロットル開度）
        _generatedTorque = _torqueCurve.Evaluate(_rpm) * _throttle;
    }
}
