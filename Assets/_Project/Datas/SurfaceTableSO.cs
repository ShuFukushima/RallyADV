using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面のタグ文字と路面アセットの組を表すクラス。SurfaceTableSO の一覧で使う
/// </summary>
[System.Serializable]
public class SurfaceTableEntry
{
    [Tooltip("地面の GameObject に設定するタグ名")]
    [SerializeField] private string _tag;

    [Tooltip("タグに対応する路面データ（SurfaceDataSO）")]
    [SerializeField] private SurfaceDataSO _surface;

    /// <summary>地面のタグ名</summary>
    public string Tag => _tag;

    /// <summary>タグに対応する路面データ</summary>
    public SurfaceDataSO Surface => _surface;
}

/// <summary>
/// 地面のタグ文字から路面アセットを引く表を持つ ScriptableObject。全車で共通の1つを使う
/// </summary>
[CreateAssetMenu(fileName = "SurfaceTable", menuName = "ScriptableObjects/SurfaceTable")]
public class SurfaceTableSO : ScriptableObject
{
    [Tooltip("地面のタグと路面データの対応表")]
    [SerializeField] private List<SurfaceTableEntry> _entries = new List<SurfaceTableEntry>();

    [Tooltip("最低グリップ値（計算結果が下回った場合や、表に無いタグの地面で使う）")]
    [SerializeField] private float _minimumGrip = 0.2f;

    /// <summary>最低グリップ値</summary>
    public float MinimumGrip => _minimumGrip;

    /// <summary>
    /// 指定したタグに対応する路面データを探して返す。GripController から呼び出す
    /// </summary>
    /// <param name="tag">地面のタグ文字</param>
    /// <param name="surface">見つかった路面データ（見つからない・未割り当ての場合は null）</param>
    /// <returns>タグが表にあり、かつ路面データが割り当てられていれば true。そうでなければ false</returns>
    public bool TryGetSurface(string tag, out SurfaceDataSO surface)
    {
        SurfaceTableEntry entry = FindEntry(tag);

        // タグに一致する行があり、かつ路面アセットが割り当てられている場合のみ有効とする
        if (entry != null && entry.Surface != null)
        {
            surface = entry.Surface;
            return true;
        }

        // 表に無い、または路面アセットが未割り当て（null）の場合は false を返す
        surface = null;
        return false;
    }

    /// <summary>
    /// 指定したタグの行が表に存在するかを返す（路面が未割り当てかどうかの判別用）。GripController から呼び出す
    /// </summary>
    /// <param name="tag">地面のタグ文字</param>
    /// <returns>表にそのタグの行があれば true、無ければ false</returns>
    public bool HasTag(string tag)
    {
        return FindEntry(tag) != null;
    }

    /// <summary>
    /// タグ文字に一致するエントリを表から探す
    /// </summary>
    /// <param name="tag">検索するタグ文字</param>
    /// <returns>一致したエントリ。見つからなければ null</returns>
    private SurfaceTableEntry FindEntry(string tag)
    {
        // タグが空、または表自体が未初期化であれば検索しない
        if (string.IsNullOrEmpty(tag) || _entries == null)
        {
            return null;
        }

        // 先頭からタグ文字が一致する行を探す
        for (int i = 0; i < _entries.Count; i++)
        {
            SurfaceTableEntry entry = _entries[i];
            if (entry != null && entry.Tag == tag)
            {
                return entry;
            }
        }

        return null;
    }
}
