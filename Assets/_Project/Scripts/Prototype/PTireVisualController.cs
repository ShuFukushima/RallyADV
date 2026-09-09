using UnityEngine;

public class PTireVisualController : MonoBehaviour
{
    Vector3 _pos;        //タイヤの座標を管理する変数
    Quaternion _rot;     //タイヤの回転を管理する変数
    [SerializeField] WheelCollider _wc;   //それぞれのWheelCollider
    [SerializeField] private bool _isRight = false; // 左右で切り替え
    Transform _wheel;    //それぞれのタイヤ



    void Start()
    {
        _wheel = this.transform;
    }

    void Update()
    {
        _wc.GetWorldPose(out _pos, out _rot);  //WheelColliderから位置と回転情報を取得
        _wheel.transform.position = _pos;     //タイヤの位置を指定

        // タイヤが右についているか左についているかで分ける
        if (_isRight)
        {
            _wheel.transform.rotation = _rot * Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            _wheel.transform.rotation = _rot;     //タイヤの角度を指定
        }

    }
}

