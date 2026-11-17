# S001 タイヤと路面でグリップを変える

- 状態: 確定
- 根拠の決定: DEC-1 タイヤと路面の組み合わせ方(2026-11-11)、DEC-55 路面の種類は地面のタグで見分ける(2026-11-05)、「M1 で作るタイヤは安いタイヤとハイグリップタイヤの2種類。相性はまだ使わない」(2026-11-17)
- 作成日: 2026-11-17(確定日: 2026-11-17)

## 1. 起きてほしいこと

- 同じ路面でも、タイヤを替えるとグリップの差が体で分かる。
- 乾いたターマックから濡れたターマックに入ると、滑りやすくなる。
- あとで雪・氷・グラベルの路面や、ほかのタイヤを足すときに、作りを直さなくていい。
- 今つくるタイヤは「安いタイヤ」と「ハイグリップタイヤ」の2種類(2026-11-17 ユーザー決定。一般の車なので「雨用」という区分は使わない)。

## 2. 選んだ案と理由

- 選んだ案: グリップ = 車の基本値 × タイヤの強さ × 路面との相性 × 路面の値。相性を書いていない路面は 1.0。摩擦を変える仕事は GripController にまとめる(DEC-1 の案C と案3c)。
- 理由: 掛け算だけだと、路面によってタイヤの順位が入れ替わらない。相性を1つ足せば入れ替えられ、書いていない路面は 1.0 なので、路面を足しても全部のタイヤを書き直さなくていい。

## 3. 作るもの

### SurfaceDataSO(新規)

- 置き場所: `Assets/_Project/Datas/SurfaceDataSO.cs`
- 役割: 路面1種類の値を持つ。路面1種類につきアセット1つ。
- 持つ値(Inspector で入れる):
  - `_value`(float): 路面の値。乾いたターマックを 1.0 とした比。
- 外に見せるもの: `public float Value => _value;`

### TireDataSO(新規)

- 置き場所: `Assets/_Project/Datas/TireDataSO.cs`
- 役割: タイヤ1種類の値を持つ。タイヤ1種類につきアセット1つ。
- 持つ値(Inspector で入れる):
  - `_grip`(float): タイヤの強さ。基準を 1.0 とした比。
  - `_affinities`(リスト): 路面との相性の一覧。1行は「路面(SurfaceDataSO への参照)」と「相性(float)」の組。この組は同じファイルに `[System.Serializable]` のクラス `SurfaceAffinity` として作り、フィールドは規約1のとおり `[SerializeField] private` にする。
- 外に見せるもの:
  - `public float Grip => _grip;`
  - `public float GetAffinity(SurfaceDataSO surface)`: 一覧にその路面があれば相性を、なければ 1.0 を返す。GripController から呼ぶ。

### SurfaceTableSO(新規)

- 置き場所: `Assets/_Project/Datas/SurfaceTableSO.cs`
- 役割: 地面のタグの文字から路面アセットを引く表。アセットは1つだけで、全部の車がこれを指す。
- 持つ値(Inspector で入れる):
  - `_entries`(リスト): 1行は「タグの文字(string)」と「路面(SurfaceDataSO への参照)」の組。組のクラス `SurfaceTableEntry` を同じファイルに作る(作り方は SurfaceAffinity と同じ)。
  - `_minimumGrip`(float): 最低グリップ。計算結果がこれを下回ったら、この値にそろえる。表に無いタグの地面でもこの値を使う。
- 外に見せるもの:
  - `public float MinimumGrip => _minimumGrip;`
  - `public bool TryGetSurface(string tag, out SurfaceDataSO surface)`: 表にタグがあれば true と路面を返す。なければ false。GripController から呼ぶ。

### GripController(新規)

- 置き場所: `Assets/_Project/Scripts/Vehicle/GripController.cs`
- 役割: 車輪ごとに、触れている地面からグリップを計算して、WheelCollider の摩擦(stiffness)に書く。摩擦を変えるのはこのクラスだけ。
- 付ける場所: CarController と同じ GameObject(`VehicleTest` シーンの車)。
- 持つ値:
  - Inspector で入れる: `_tire`(TireDataSO)、`_surfaceTable`(SurfaceTableSO)
  - Inspector で入れない: `_setting`(VehicleSettingSO。SetSetting で受け取る)、警告を出し終えた地面の記録(1つの地面につき警告1回にするため)
  - 表示用(`[Header("表示用")]`): `_forwardGrip`、`_sidewaysGrip`(float[4]、車輪ごとの最終的な値)、`_surfaceNames`(string[4]、車輪ごとの路面アセットの名前。表に無いタグなら「表に無いタグ: <タグ>」)
- 外に見せるもの:
  - `public void SetSetting(VehicleSettingSO setting)`: 車の基本値を受け取る。CarController の Awake から呼ぶ。
  - `public void GripStep(WheelCollider[] wheels)`: 1回分の計算をする。CarController の FixedUpdate から呼ぶ。
- 処理の流れ(GripStep。車輪1つずつ):
  1. `GetGroundHit` で触れている地面を調べる。false(浮いている)なら、その車輪は何もしない(前の値のまま)。
  2. 地面のタグで SurfaceTableSO を引く。
  3. 表にあれば、前後と横それぞれで計算する:
     - 前後のグリップ = `ForwardStiffness`(VehicleSettingSO)× タイヤの強さ × 相性 × 路面の値
     - 横のグリップ = `SidewaysStiffness`(VehicleSettingSO)× タイヤの強さ × 相性 × 路面の値
  4. 表に無ければ、前後・横とも最低グリップにする。エディターでの実行中だけ、その地面1つにつき1回、`Debug.LogWarning` で「SurfaceTable に無いタグ: <タグ>(地面: <名前>)」と出す。
  5. 計算結果が最低グリップを下回ったら、最低グリップにそろえる。
  6. `forwardFriction` と `sidewaysFriction` を取り出し、stiffness だけ書き換えて戻す(ほかの値は変えない)。
- 計算は1つのメソッドにまとめる。あとで摩耗の掛け算を1つ足せるようにするため(摩耗そのものは作らない)。

### CarController(変更)

- 置き場所: `Assets/_Project/Scripts/Vehicle/CarController.cs`
- 変えること:
  - `FixedUpdate` の中の、地面のタグで stiffness を書き換えている部分(`// 接地している情報を取得する` から、左右の車輪の処理の終わりまで)を消す。
  - `[SerializeField] private GripController _gripController;` を足す(Tooltip 付き)。Awake で付け忘れを確かめる(規約10)。
  - Awake で `_gripController.SetSetting(_setting);` を呼ぶ。
  - FixedUpdate で、トルク・ステアリング・ブレーキを入れたあとに `_gripController.GripStep(_wheels);` を呼ぶ。
- 変えないこと: Awake で WheelCollider に摩擦カーブを最初に入れる処理。HUD が使っている `GetGroundHit(int index)` などの公開メソッド。

## 4. 数値(すべて仮。第6週の基準挙動の調整で直す)

| 名前 | 値 | 置き場所 | メモ |
|---|---|---|---|
| 乾いたターマックの路面の値 | 1.0 | SurfaceDataSO `Surface_TarmacDry` | 基準 |
| 濡れたターマックの路面の値 | 0.5 | SurfaceDataSO `Surface_TarmacWet` | DEC-1 の仮の値。今のコードの右車輪と同じ |
| 安いタイヤの強さ | 0.8 | TireDataSO `Tire_Economy` | |
| ハイグリップタイヤの強さ | 1.2 | TireDataSO `Tire_HighGrip` | |
| 相性 | 書かない(すべて 1.0 扱い) | TireDataSO の一覧 | 今の2種類は、どの路面でもハイグリップが上なので使わない(2026-11-17 ユーザー決定)。仕組みだけ作り、7. で動くかを確かめる |
| 最低グリップ | 0.2 | SurfaceTableSO `SurfaceTable` | 0 にすると摩擦が全部消えるので、0 より大きくする |
| 車の基本値 | 1.0(前後・横とも) | VehicleSettingSO(今の `TestVehicleSO` の値) | 変えない |

この値だと、最終的なグリップは次のとおり。

| | 乾いた | 濡れた |
|---|---|---|
| 安いタイヤ | 0.8 | 0.4 |
| ハイグリップタイヤ | 1.2 | 0.6 |

## 5. 触ってよいファイル

- 変える: `Assets/_Project/Scripts/Vehicle/CarController.cs`
- 新しく作る: `Assets/_Project/Datas/SurfaceDataSO.cs`、`Assets/_Project/Datas/TireDataSO.cs`、`Assets/_Project/Datas/SurfaceTableSO.cs`、`Assets/_Project/Scripts/Vehicle/GripController.cs`
- 上記以外は変えない。

## 6. ユーザーが Unity でやること

1. Assets/Create のメニューから、次のアセットを作り、4. の値を入れる。
   - SurfaceDataSO: `Surface_TarmacDry`、`Surface_TarmacWet`
   - TireDataSO: `Tire_Economy`、`Tire_HighGrip`
   - SurfaceTableSO: `SurfaceTable`(1行目: `Ground_Tarmac_Dry` → `Surface_TarmacDry`、2行目: `Ground_Tarmac_Wet` → `Surface_TarmacWet`)
2. `VehicleTest` シーンの車に GripController を付け、`_tire` に `Tire_Economy`、`_surfaceTable` に `SurfaceTable` を入れる。
3. CarController の `_gripController` に、その GripController を入れる。
4. シーンを保存する。

## 7. 受け入れ条件

- [ ] コンパイルエラーがない
- [ ] CarController に、タグの文字を比べるコードと stiffness を書き換えるコードが残っていない
- [ ] 乾いたターマックで、GripController の表示用の値が、安いタイヤで 0.8、ハイグリップタイヤで 1.2 になる(4輪とも)
- [ ] 濡れたターマックに入ると、4輪とも 4. の表の値に変わる(左右で同じ値)
- [ ] 同じ速さで同じカーブに入ると、安いタイヤのほうが先に外へふくらむ・滑る
- [ ] タグの付いていない地面に乗ると、値が最低グリップになり、警告がその地面につき1回だけ出る
- [ ] ジャンプなどで車輪が浮いている間は、値が変わらない
- [ ] 相性の確認(一時的): `Tire_Economy` の相性の一覧に「`Surface_TarmacWet` → 0.5」を1行足すと、濡れたターマックでの値が 0.4 から 0.2 に変わる。乾いたターマックでは 0.8 のまま。確かめたら、その1行を消す

## 8. 対象外

- 摩耗(計算を1つのメソッドにまとめておくだけ)
- 選んだタイヤを車に入れる仕組み(チューニング基盤でやる。今は Inspector で差し替える)
- 雪・氷・グラベルの路面
- 数値の調整(第6週)

## 9. 未決定

- なし(数値はすべて仮で、第6週に調整する)。
