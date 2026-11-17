#if UNITY_EDITOR
using System.Collections.Generic;
#endif
using UnityEngine;

/// <summary>
/// 車輪ごとに触れている地面からグリップを計算し、WheelCollider の摩擦（stiffness）を更新する。
/// CarController と同じ車の GameObject に付ける
/// </summary>
public class GripController : MonoBehaviour
{
    [Tooltip("装着しているタイヤのデータ（TireDataSO）")]
    [SerializeField] private TireDataSO _tire;

    [Tooltip("地面のタグから路面データを引く対応表（SurfaceTableSO）")]
    [SerializeField] private SurfaceTableSO _surfaceTable;

    /// <summary>車両の基本設定。CarController の Awake から SetSetting で受け取る</summary>
    private VehicleSettingSO _setting;

#if UNITY_EDITOR
    /// <summary>警告を1回だけ出すために、警告済みの地面 GameObject を記録する</summary>
    private readonly HashSet<GameObject> _warnedGrounds = new HashSet<GameObject>();
#endif

    [Header("表示用")]
    [Tooltip("車輪ごとの最終的な前後グリップ（Play 中の確認用で、入力しない）")]
    [SerializeField] private float[] _forwardGrip = new float[4];

    [Tooltip("車輪ごとの最終的な横グリップ（Play 中の確認用で、入力しない）")]
    [SerializeField] private float[] _sidewaysGrip = new float[4];

    [Tooltip("車輪ごとの接地中の路面アセット名（Play 中の確認用で、入力しない）")]
    [SerializeField] private string[] _surfaceNames = new string[4];

    /// <summary>
    /// Inspector での参照の付け忘れを確認する
    /// </summary>
    private void Awake()
    {
        // タイヤまたは路面テーブルが未設定であればエラーを出して知らせる
        if (_tire == null)
        {
            Debug.LogError("GripControllerにTireDataSOが設定されていません");
        }
        if (_surfaceTable == null)
        {
            Debug.LogError("GripControllerにSurfaceTableSOが設定されていません");
        }
    }

    /// <summary>
    /// 車両の基本値（VehicleSettingSO）を受け取る。CarController の Awake から呼び出す
    /// </summary>
    /// <param name="setting">車両個別の設定アセット</param>
    public void SetSetting(VehicleSettingSO setting)
    {
        _setting = setting;
    }

    /// <summary>
    /// 各車輪の接地地面からグリップを計算し、WheelCollider の stiffness を更新する。
    /// CarController の FixedUpdate から呼び出す
    /// </summary>
    /// <param name="wheels">車の 4 輪の WheelCollider 配列</param>
    public void GripStep(WheelCollider[] wheels)
    {
        // 必要な設定や配列が揃っていなければ処理を止める
        if (wheels == null || _tire == null || _surfaceTable == null || _setting == null)
        {
            return;
        }

        // 車輪1つずつについて、接地している路面に応じたグリップを計算・反映する
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelCollider wheel = wheels[i];
            if (wheel == null)
            {
                continue;
            }

            // 浮いている（接地していない）車輪は何もしない（グリップ値も表示用も前の値のまま保つ）
            if (!wheel.GetGroundHit(out WheelHit hit) || hit.collider == null)
            {
                continue;
            }

            GameObject groundObject = hit.collider.gameObject;
            string groundTag = groundObject.tag;

            float forwardGrip;
            float sidewaysGrip;
            string surfaceName;

            // 地面のタグで路面テーブルを引く
            if (_surfaceTable.TryGetSurface(groundTag, out SurfaceDataSO surface))
            {
                // 表にあれば、タイヤと路面の相性を取得して前後・横それぞれのグリップを計算する
                float affinity = _tire.GetAffinity(surface);
                forwardGrip = CalculateGrip(_setting.ForwardStiffness, _tire.Grip, affinity, surface.Value, _surfaceTable.MinimumGrip);
                sidewaysGrip = CalculateGrip(_setting.SidewaysStiffness, _tire.Grip, affinity, surface.Value, _surfaceTable.MinimumGrip);
                surfaceName = surface.name;
            }
            else
            {
                // 表に無いタグ、または路面が未割り当ての場合は前後・横とも最低グリップにする
                forwardGrip = _surfaceTable.MinimumGrip;
                sidewaysGrip = _surfaceTable.MinimumGrip;

                // タグ自体は表にあるかで「路面未設定」と「表に無いタグ」を見分ける
                bool hasTagInTable = _surfaceTable.HasTag(groundTag);
                surfaceName = hasTagInTable
                    ? $"路面未設定: {groundTag}"
                    : $"表に無いタグ: {groundTag}";

                // エディター実行中のみ、同じ地面 GameObject につき1回だけ警告を出す
                LogMissingSurfaceWarningOnce(groundObject, groundTag, hasTagInTable);
            }

            // WheelCollider の forwardFriction / sidewaysFriction の stiffness だけを書き換えて戻す
            ApplyWheelFriction(wheel, forwardGrip, sidewaysGrip);

            // Play 中に Inspector で確認できるように表示用配列へ記録する
            if (i < _forwardGrip.Length)
            {
                _forwardGrip[i] = forwardGrip;
                _sidewaysGrip[i] = sidewaysGrip;
                _surfaceNames[i] = surfaceName;
            }
        }
    }

    /// <summary>
    /// 車の基本値・タイヤの強さ・相性・路面の値から最終グリップを計算する。
    /// あとで摩耗の掛け算を1つ足せるように、計算処理を1つのメソッドにまとめている。GripStep から呼び出す
    /// </summary>
    /// <param name="baseStiffness">車の基本摩擦倍率（VehicleSettingSO の値）</param>
    /// <param name="tireGrip">タイヤの強さ（TireDataSO の Grip）</param>
    /// <param name="affinity">タイヤと路面の相性倍率</param>
    /// <param name="surfaceValue">路面の値（SurfaceDataSO の Value）</param>
    /// <param name="minimumGrip">最低グリップ値（SurfaceTableSO の MinimumGrip）</param>
    /// <returns>最低グリップ以上にそろえた最終グリップ値</returns>
    private float CalculateGrip(float baseStiffness, float tireGrip, float affinity, float surfaceValue, float minimumGrip)
    {
        // 仕様書 S001 の 2. 選んだ案と理由: グリップ = 車の基本値 × タイヤの強さ × 路面との相性 × 路面の値
        float grip = baseStiffness * tireGrip * affinity * surfaceValue;

        // 計算結果が最低グリップを下回ったら、摩擦が消えないように最低グリップにそろえる
        if (grip < minimumGrip)
        {
            grip = minimumGrip;
        }

        return grip;
    }

    /// <summary>
    /// WheelCollider の forwardFriction と sidewaysFriction の stiffness のみを書き換える。GripStep から呼び出す
    /// </summary>
    /// <param name="wheel">書き換え対象の WheelCollider</param>
    /// <param name="forwardGrip">適用する前後方向の stiffness</param>
    /// <param name="sidewaysGrip">適用する横方向の stiffness</param>
    private void ApplyWheelFriction(WheelCollider wheel, float forwardGrip, float sidewaysGrip)
    {
        // 既存のカーブパラメータを壊さないように構造体を取り出し、stiffness だけ書き換えて戻す
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.stiffness = forwardGrip;
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = sidewaysGrip;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    /// <summary>
    /// 表に無いタグや路面未設定の地面に対して、エディター実行中のみ地面 GameObject 1つにつき1回警告を出す。
    /// GripStep から呼び出す
    /// </summary>
    /// <param name="groundObject">車輪が触れている地面の GameObject</param>
    /// <param name="groundTag">地面のタグ名</param>
    /// <param name="hasTagInTable">表にタグ自体は登録されているか（true なら路面未設定）</param>
    private void LogMissingSurfaceWarningOnce(GameObject groundObject, string groundTag, bool hasTagInTable)
    {
#if UNITY_EDITOR
        // すでにこの地面 GameObject で警告を出していれば重複させない
        if (!_warnedGrounds.Add(groundObject))
        {
            return;
        }

        if (hasTagInTable)
        {
            // タグは表に一致したが、路面データ（SurfaceDataSO）が未割り当てだった場合
            Debug.LogWarning($"SurfaceTable の路面が未設定: {groundTag}");
        }
        else
        {
            // 表に登録されていないタグの地面に乗った場合
            Debug.LogWarning($"SurfaceTable に無いタグ: {groundTag}(地面: {groundObject.name})");
        }
#endif
    }
}
