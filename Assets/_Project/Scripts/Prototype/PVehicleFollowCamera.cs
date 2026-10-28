using UnityEngine;

public class PVehicleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _target; // 追う相手
    [SerializeField] private float _distance;   // 後方何m離れるか
    [SerializeField] private float _height;     // 車より何m上に置くか
    [SerializeField] private float _lookHeight; // 車の原点から何m上を見るか
    [SerializeField] private float _smoothTime; // 狙いの位置に追いつくまでのおおよその秒数
    [SerializeField] private Vector3 _velocity; // カメラの今の速さ

    private void LateUpdate()
    {
        // 車の前の向きを取る
        Vector3 vehicleForward = _target.forward;
        // 前向きの内、上下成分を0にして長さを１にする
        vehicleForward.y = 0f;
        vehicleForward = vehicleForward.normalized;
        // 狙いの位置を求める
        Vector3 targetPosition = _target.position - vehicleForward * _distance + Vector3.up * _height;

        // カメラ位置を更新する
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);

        // カメラを車の位置 + 真上 * _lookHeightの点へ向ける
        transform.LookAt(_target.position + Vector3.up * _lookHeight);
    }
}
