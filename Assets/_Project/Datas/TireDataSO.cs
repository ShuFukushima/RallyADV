using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 路面1つとタイヤの相性の組を表すクラス。TireDataSO の一覧で使う
/// </summary>
[System.Serializable]
public class SurfaceAffinity
{
    [Tooltip("相性を設定する対象の路面アセット")]
    [SerializeField] private SurfaceDataSO _surface;

    [Tooltip("路面との相性倍率（書かれていない路面は 1.0 扱い）")]
    [SerializeField] private float _affinity = 1.0f;

    /// <summary>相性を設定する対象の路面アセット</summary>
    public SurfaceDataSO Surface => _surface;

    /// <summary>路面との相性倍率</summary>
    public float Affinity => _affinity;
}

/// <summary>
/// タイヤ1種類の値を持つ ScriptableObject。タイヤ1種類につきアセット1つを作成する
/// </summary>
[CreateAssetMenu(fileName = "TireData", menuName = "ScriptableObjects/TireData")]
public class TireDataSO : ScriptableObject
{
    [Tooltip("タイヤの強さ（基準を 1.0 とした比）")]
    [SerializeField] private float _grip = 1.0f;

    [Tooltip("路面ごとの相性の一覧（未記載の路面は 1.0 として扱われる）")]
    [SerializeField] private List<SurfaceAffinity> _affinities = new List<SurfaceAffinity>();

    /// <summary>タイヤの強さ（基準を 1.0 とした比）</summary>
    public float Grip => _grip;

    /// <summary>
    /// 指定した路面との相性を返す。一覧に無ければ 1.0 を返す。GripController から呼び出す
    /// </summary>
    /// <param name="surface">相性を調べたい路面データ</param>
    /// <returns>路面との相性倍率（未登録または未設定なら 1.0f）</returns>
    public float GetAffinity(SurfaceDataSO surface)
    {
        // 調べる路面または相性一覧が未設定であれば、基準の 1.0 を返す
        if (surface == null || _affinities == null)
        {
            return 1.0f;
        }

        // 一覧を先頭から確認し、指定した路面と一致する相性を返す
        for (int i = 0; i < _affinities.Count; i++)
        {
            SurfaceAffinity entry = _affinities[i];
            if (entry != null && entry.Surface == surface)
            {
                return entry.Affinity;
            }
        }

        // 仕様書 S001 の 2. 選んだ案と理由: 相性を書いていない路面は 1.0 とする
        return 1.0f;
    }
}
