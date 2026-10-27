using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 車両情報が取得できているかを確認する仮のスクリプト
/// </summary>
public class PVehicleInfoHUD : MonoBehaviour
{
    [SerializeField] private CarController _car;
    [SerializeField] private EngineModel _engine;
    [SerializeField] private DriveTrainModel _driveTrain;

    [Header("HUDテキスト")]
    [SerializeField] private Text _speed;
    [SerializeField] private Text _rpm;
    [SerializeField] private Text _nowGear;
    [SerializeField] private Text[] _isGrounded;
    [SerializeField] private Text[] _forwardSlip;
    [SerializeField] private Text[] _sidewaySlip;

    private void Update()
    {
        // 車両情報
        _speed.text = _car.SpeedKmh.ToString("000") + " km/h";
        _rpm.text = _engine.EngineRPM.ToString("0000") + " rpm";
        _nowGear.text = "now gear : " + _driveTrain.CurrentGear.ToString();

        for(int i = 0;  i < 4; i++)
        {
            // 接地判定
            if(_car.IsWheelGrounded(i))
            {
                _isGrounded[i].text = "yes";
            }
            else
            {
                _isGrounded[i].text = "no";
            }
            // 縦横スリップ
            _forwardSlip[i].text = _car.GetForwardSlip(i).ToString("0.000");
            _sidewaySlip[i].text = _car.GetSidewaysSlip(i).ToString("0.000");
        }
    }
}
