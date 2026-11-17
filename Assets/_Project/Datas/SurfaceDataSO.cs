using UnityEngine;

/// <summary>
/// 路面1種類の値を持つ ScriptableObject。路面1種類につきアセット1つを作成する
/// </summary>
[CreateAssetMenu(fileName = "SurfaceData", menuName = "ScriptableObjects/SurfaceData")]
public class SurfaceDataSO : ScriptableObject
{
    [Tooltip("路面の値（乾いたターマックを 1.0 とした比）")]
    [SerializeField] private float _value = 1.0f;

    /// <summary>路面の値（乾いたターマックを 1.0 とした比）</summary>
    public float Value => _value;
}
