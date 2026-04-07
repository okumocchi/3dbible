using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Splines;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using DG.Tweening;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class UIController : MonoBehaviour
{
    [SerializeField] CinemachineCamera mainCamera;

    // 慣性処理用の変数
    private bool inertiaFlag = false; // 慣性による処理が発生中かどうか
    private Vector2 rotationInertia = Vector2.zero; // 回転の慣性
    private Vector2 panInertia = Vector2.zero; // 並行移動の慣性
    private float zoomInertia = 0f; // ズームの慣性
    private float inertiaDecay = 0.90f; // 慣性の減衰率
    private float minInertiaThreshold = 0.01f; // 慣性の最小閾値

    private bool isHelpShown = false;

    private Transform goalCamera = null;

    // 自動視点移動経過秒数
    private float elapsedTime = 999.0f; // 高速視点移動用
    private float elapsedTime2 = 999.0f; // 低速視点移動用

    //private Vector3 startPos;
    //private Vector3 startAngle;

    // カメラの回転速度を格納する変数
    public Vector2 rotationSpeed;
    // マウス移動方向とカメラ回転方向を反転する判定フラグ
    public bool reverse;

    [SerializeField] private GameObject compass;
    [SerializeField] private GameObject cameraButton;
    [SerializeField] private GameObject speedSliderPanel;
    [SerializeField] private GameObject helpView;
    [SerializeField] private GameObject bibleButton;
    [SerializeField] private RectTransform helpViewTransform;
    [SerializeField] private ScrollRect helpViewScrollRect;


    private Slider speedSlider;

    // マウス座標を格納する変数
    private Vector2 lastMousePosition;
    // カメラの角度を格納する変数（初期値に0,0を代入）
    private Vector2 newAngle = new Vector2(0, 0);

    private bool isForwardButtonDown = false;
    private bool isBackButtonDown = false;


    // MAP Controll
    [SerializeField] private GameObject lv4Map;
    [SerializeField] private GameObject lv6Map;
    [SerializeField] private GameObject lv7Map;
    [SerializeField] private GameObject humanPrefab;


    private GameObject currentMap = null;
    private GameObject nextMap = null;
    private Bounds mapBounds;
    private float switchMapTime = 999.0f; // フェードアニメーション用

    [SerializeField] private GameObject renderTexture;
    [SerializeField] private Camera renderTextureCamera;

    private Transform preMapCameraGoal = null; // マップ切り替え時、元のマップのカメラ移動用（目的位置）

    private float alpha = 0.0f;
    private float alphaDlt = 0.05f;

    private GameObject placeLabels = null;

    // コンパスをクリックした時のカメラ位置
    private Marker inCamera = null;

    // マップ切り替え時の視点移動情報
    private Marker fromCameraPre = null;
    private Marker toCameraPre = null;
    private Marker fromCamera = null;
    private Marker toCamera = null;

    // マウスが動いたかどうか（タップ判定のため）
    private bool mouseMoveFlag;

    private float lastClickedTime = 0;
    private float clickInterval = 99999;

    // エピソードプレイ時にカメラが追随するオブジェクト
    private Transform followTarget = null;
    private bool followMode = false; // 1: 自動追随する | 0: しない
    private float preY = 0;
    private float followCameraHeight = 60.0f;

    private bool inOperation = false; // ボタンやスライダーを操作中かどうか
    public static float moveSpeed = 25.0f; // キャラ移動速度（1フレームの移動量）
    private Marker startCameraMarker; // 表示中マップのデフォルト開始位置

    //private DeviceOrientation lastOrientation; // 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //lastOrientation = Input.deviceOrientation; // デバイスの向きをとっておく
        currentMap = lv4Map;
        UpdateMapBounds();

        var startCamera = currentMap.transform.Find("Camera4");
        startCameraMarker = new Marker(startCamera);
        mainCamera.transform.position = startCamera.transform.position;
        mainCamera.transform.localEulerAngles = startCamera.transform.localEulerAngles;
        //CopyTransform(inCamera, startCamera.transform);// = 
        //inCamera = mainCamera.Instanciate();

        inCamera = new Marker(startCamera.transform);
        toCamera = new Marker(startCamera.transform);

        // オブジェクトのローカル回転を更新
        //mainCamera.transform.localEulerAngles = currentEulerAngles;

        // カメラの角度を変数"newAngle"に格納
        newAngle = mainCamera.transform.localEulerAngles;

        // 初期状態はLv4map
        //lv4Map.SetActive(true);
        //lv6Map.SetActive(false);
        cameraButton.SetActive(false);

        speedSlider = speedSliderPanel.GetComponentInChildren<Slider>();
        speedSliderPanel.SetActive(false);

        EpisodeBase.SetObjects(lv4Map, lv6Map, lv7Map, humanPrefab);
        Action<Transform> setFollowTarget = SetFollowTarget;
        EpisodeBase.SetFollowTarget = setFollowTarget;
    }


    //
    // InputSystem
    //
    // タップ用Action
    [SerializeField] private InputActionProperty _tapAction;
    // マルチタップ用Action
    [SerializeField] private InputActionProperty _multiTapAction;
    // タッチ位置取得用Action
    //[SerializeField] private InputActionProperty _touch0Action;
    // タップが行われたかどうか(イベントの重複検知防止用)
    private bool _isTapPerformed;

    private void OnEnable()
    {
        // Actionのコールバック登録
        _tapAction.action.performed += TapActionCallback;
        _multiTapAction.action.performed += MultiTapActionCallback;
        _multiTapAction.action.canceled += MultiTapActionCallback;

        // Actionの有効化
        _multiTapAction.action.Enable();
        _tapAction.action.Enable();
       // _touch0Action.action.Enable();
    }

    private void OnDisable()
    {
        // Actionのコールバック登録解除
        _tapAction.action.performed -= TapActionCallback;
        _multiTapAction.action.performed -= MultiTapActionCallback;
        _multiTapAction.action.canceled -= MultiTapActionCallback;

        // Actionの無効化
        _multiTapAction.action.Disable();
        _tapAction.action.Disable();
        //_touch0Action.action.Disable();
    }

    // タップ時のコールバック
    private void TapActionCallback(InputAction.CallbackContext context)
    {
        // ダブルタップ時にここでタップ座標を覚えておく
        // TouchState touchState = _touch0Action.action.ReadValue<TouchState>();
        // Vector2 pos = touchState.position;
        // if (pos.x > 0 && pos.y > 0) // ２回目には(0,0)になるため
        // {
        //     lastTappedPos = pos;
        // }
        
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                // タップがまだされておらず、マルチタップが行われていない場合は
                // タップが行われたと判定
                if (!_isTapPerformed && _multiTapAction.action.phase != InputActionPhase.Waiting)
                {
                    _isTapPerformed = true;
                    OnTap();
                }

                break;
        }
    }

    // マルチタップ時のコールバック
    Vector2 lastTappedPos;
    private void MultiTapActionCallback(InputAction.CallbackContext context)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                OnTapCanceled();
                OnMultiTap();
                break;

            case InputActionPhase.Canceled:
                // マルチタップが中断された場合
                _isTapPerformed = false;
                break;
        }
    }

    //
    // Input Action
    //
    // ２本指のタッチ情報
    private TouchState _touchState0;
    private TouchState _touchState1;
    private int singleTouchCount = 0; // タッチ時にフォローモードが簡単に解除されないように遊びを設ける

    // Touch #0 入力
    public void OnTouch0(InputAction.CallbackContext context)
    {
        _touchState0 = context.ReadValue<TouchState>();

        Vector2 pos = _touchState0.position;
        if (pos.x > 0 && pos.y > 0) // ２回目には(0,0)になるため
        {
            lastTappedPos = pos;
        }

        if (_touchState0.phase == TouchPhase.Ended)
        {
            singleTouchCount = 0;
            // タッチ終了時に慣性を設定
            if (_touchState0.isInProgress)
            {
                var delta0 = _touchState0.delta;
            }
        } else {
            // タッチが終了した場合に、deltaが0になるため、その前にとっておく
            var delta0 = _touchState0.delta;
                var coe = mainCamera.transform.position.y / 3000;

            // 回転の慣性を設定
            rotationInertia = new Vector2(
                delta0.x * rotationSpeed.y * 0.3f,
                -delta0.y * rotationSpeed.x * 0.3f
            );

            // 並行移動の慣性を設定
            // panInertia = new Vector2(
            //     -delta0.x * coe,
            //     -delta0.y * coe
            // );
          
        }

        // スワイプまたはピンチ処理
        OnPinch();
    }

    // Touch #1 入力
    public void OnTouch1(InputAction.CallbackContext context)
    {
        _touchState1 = context.ReadValue<TouchState>();
        
        // // タッチ終了時にピンチの慣性を設定
        // if (/*_touchState1.phase == TouchPhase.Ended && */_touchState0.isInProgress)
        // {
        //     var delta0 = _touchState0.delta;
        //     var delta1 = _touchState1.delta;
        //     var coe = mainCamera.transform.position.y / 1000;

        //     // ピンチ処理の際にカメラを回転させない            
        //     ResetInertia();
        //     singleTouchCount = 0;

        //     // ピンチ操作の慣性を設定
        //     var pinchDelta = Vector3.Distance(_touchState0.position, _touchState1.position) - 
        //                    Vector3.Distance(_touchState0.position - delta0, _touchState1.position - delta1);
        //     zoomInertia = pinchDelta * 2.0f * coe;
        // }
        
        // ピンチ処理
        OnPinch();
    }

    // ピンチ判定処理
    private void OnPinch()
    {
        if (inOperation) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        
        bool isTouching = _touchState0.isInProgress;
        
        if (isTouching)
        {
            
            var pos0 = _touchState0.position;// タッチ位置（スクリーン座標）
            var delta0 = _touchState0.delta;// 移動量（スクリーン座標）
            var prevPos0 = pos0 - delta0;// 移動前の位置（スクリーン座標）
            var coe = mainCamera.transform.position.y / 1500; // カメラの高さ（30〜1000くらい）で移動量を調整する
            
            if (_touchState1.isInProgress) // マルチタッチ時
            { 
                bibleButton.GetComponent<HeaddingsController>().showBookButtons(false);

                // ピンチ操作
                singleTouchCount = 0;
                var pos1 = _touchState1.position; // タッチ位置（スクリーン座標）
                var delta1 = _touchState1.delta; // 移動量（スクリーン座標）                  
                var prevPos1 = pos1 - delta1; // 移動前の位置（スクリーン座標）

                // ピンチ操作
                // タッチ２点間の距離の変化量
                var pinchDelta = Vector3.Distance(pos0, pos1) - Vector3.Distance(prevPos0, prevPos1);

                var cameraPos = mainCamera.transform.position;
                float lowLimit = 12f;
                float highLimit = 1040f;
                if (followMode)
                {
                    followCameraHeight = Mathf.Max(lowLimit, followCameraHeight - pinchDelta * 2.0f * coe);
                }
                else
                {
                    cameraPos += mainCamera.transform.forward * pinchDelta * 1.2f * coe;
                    if (cameraPos.y < lowLimit) cameraPos.y = lowLimit;
                    else if (cameraPos.y > highLimit) cameraPos.y = highLimit;
                }

                // ピンチ処理の際にカメラを回転させない            
                ResetInertia();
                singleTouchCount = 0;

                // ピンチ操作の慣性を設定
                //var pinchDelta = Vector3.Distance(_touchState0.position, _touchState1.position) - 
                //            Vector3.Distance(_touchState0.position - delta0, _touchState1.position - delta1);
                zoomInertia = pinchDelta * 1.2f * coe;


                //２指スワイプ操作
                // 画面座標系での移動量を取得
                var center = (pos0 + pos1) * 0.5f;
                var prevCenter = (prevPos0 + prevPos1) * 0.5f;
                var screenDelta = center - prevCenter;
                
                // カメラの右方向と上方向のベクトルを取得
                Vector3 cameraRight = mainCamera.transform.right;
                Vector3 cameraUp = mainCamera.transform.up;
                
                // 画面の左右移動をカメラの右方向に、上下移動をカメラの上方向に変換
                Vector3 worldDelta = cameraRight * screenDelta.x + cameraUp * screenDelta.y;
                
                // 地面に投影（Y成分を0にする）
                worldDelta.y = 0;
                
                // 係数を適用してカメラ位置を更新（移動方向を反転）
                cameraPos -= worldDelta * coe * 0.5f;

                // マップ境界内に制限
                if (cameraPos.x > mapBounds.max.x) cameraPos.x = mapBounds.max.x;
                if (cameraPos.x < mapBounds.min.x) cameraPos.x = mapBounds.min.x;
                if (cameraPos.z > mapBounds.max.z) cameraPos.z = mapBounds.max.z;
                if (cameraPos.z < mapBounds.min.z) cameraPos.z = mapBounds.min.z;

                mainCamera.transform.position = cameraPos;
            }
            else if (singleTouchCount++ > 2)
            { // シングルタッチスワイプ操作
                activateCameraButton(true);
                bibleButton.GetComponent<HeaddingsController>().showBookButtons(false);

                // Y軸の回転：マウスドラッグ方向に視点回転
                // マウスの水平移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.y += delta0.x * rotationSpeed.y * 0.3f;
                // X軸の回転：マウスドラッグ方向に視点回転
                // マウスの垂直移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.x -= delta0.y * rotationSpeed.x * 0.3f;
                if (newAngle.x < 4)
                { // 340
                    newAngle.x = 4;
                }
                else if (newAngle.x > 90)
                { // 450
                    newAngle.x = 90;
                }
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;

                // マウス座標を変数"lastMousePosition"に格納
                //lastMousePosition = Input.mousePosition;
            }
        }
        

        // タッチが終了した場合、慣性処理を開始
        if (!isTouching && (rotationInertia.magnitude > minInertiaThreshold || 
                           panInertia.magnitude > minInertiaThreshold || 
                           Mathf.Abs(zoomInertia) > minInertiaThreshold))
        {
            inertiaFlag = true;
            ApplyInertia();
        }
    }

    // タップされた時の処理
    private void OnTap()
    {
        //print("タップされた！");
    }

    // タップのロールバック処理
    private void OnTapCanceled()
    {
        //print("タップがキャンセルされた！");
    }

    // マルチタップされた時の処理
    private void OnMultiTap()
    {
        //print("マルチタップされた！");
        ZoomTo();
    }

    // 慣性処理を適用するメソッド
    private void ApplyInertia()
    {
        if (inOperation) return;
        inertiaFlag = false;
        
        var cameraPos = mainCamera.transform.position;
        float lowLimit = 12f;
        float highLimit = 1040f;
        var coe = mainCamera.transform.position.y / 1500;
        
        // 回転の慣性処理
        if (rotationInertia.magnitude > minInertiaThreshold)
        {
            newAngle.y += rotationInertia.x;
            newAngle.x += rotationInertia.y;
            
            if (newAngle.x < 4) newAngle.x = 4;
            else if (newAngle.x > 90) newAngle.x = 90;
            
            mainCamera.transform.localEulerAngles = newAngle;
            rotationInertia *= inertiaDecay;
            inertiaFlag = true;
        }
        
        // 並行移動の慣性処理
        // if (panInertia.magnitude > minInertiaThreshold)
        // {
        //     var cx = cameraPos.x + panInertia.x;
        //     var cz = cameraPos.z + panInertia.y;
            
        //     if (cx > mapBounds.max.x) cx = mapBounds.max.x;
        //     if (cx < mapBounds.min.x) cx = mapBounds.min.x;
        //     if (cz > mapBounds.max.z) cz = mapBounds.max.z;
        //     if (cz < mapBounds.min.z) cz = mapBounds.min.z;
            
        //     cameraPos.x = cx;
        //     cameraPos.z = cz;
        //     mainCamera.transform.position = cameraPos;
        //     panInertia *= inertiaDecay;
        //     inertiaFlag = true;
        // }
        
        // ズームの慣性処理
        if (Mathf.Abs(zoomInertia) > minInertiaThreshold)
        {
            if (followMode)
            {
                followCameraHeight = Mathf.Max(lowLimit, followCameraHeight - zoomInertia);
            }
            else
            {
                cameraPos += mainCamera.transform.forward * zoomInertia;
                if (cameraPos.y < lowLimit) cameraPos.y = lowLimit;
                else if (cameraPos.y > highLimit) cameraPos.y = highLimit;
                mainCamera.transform.position = cameraPos;
            }
            zoomInertia *= inertiaDecay;
            inertiaFlag = true;
        }
    }




    void UpdateMapBounds()
    {
        Terrain terrain = currentMap.transform.Find("Terrain").gameObject.GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;

        // ローカル座標のBounds
        Bounds localBounds = data.bounds;

        // ワールド座標への変換
        Vector3 worldCenter = terrain.transform.position + localBounds.center;
        Bounds worldBounds = new Bounds(worldCenter, localBounds.size);
        mapBounds = worldBounds;
    }

    void SetFollowTarget(Transform target)
    {
        followTarget = target;
        fromCamera = new Marker(mainCamera.transform);
        elapsedTime = 0.0f;
    }

    public void CopyTransform(Transform to, Transform from)
    {
        to.position = from.position;
        to.localEulerAngles = from.localEulerAngles;
    }

    void Update()
    {
        if (inertiaFlag)
        {
            ApplyInertia();
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // 開発用カメラ操作: Wキーで前進、Sキーで後退
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            float debugMoveSpeed = 100f; // 必要に応じて速度を調整してください
            if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed)
            {
                mainCamera.transform.position += mainCamera.transform.forward * debugMoveSpeed * Time.deltaTime;
                ResetInertia(); // 前進時に慣性をリセット
            }
            if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed)
            {
                mainCamera.transform.position -= mainCamera.transform.forward * debugMoveSpeed * Time.deltaTime;
                ResetInertia(); // 後退時に慣性をリセット
            }
        }
#endif
        //DeviceOrientation current = Input.deviceOrientation;

        // 無効な状態はスキップ
        // if (current == DeviceOrientation.Unknown || current == DeviceOrientation.FaceUp || current == DeviceOrientation.FaceDown)
        //     return;

        // if (current != lastOrientation)
        // {
        //     lastOrientation = current;
        //     OnOrientationChanged(current);
        // }


        //     // 画面サイズが変わったら再設定（必要なら）
        //     if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        //     {
        //         UpdateRenderTextureSize();
        //     }
        // }
        // void UpdateRenderTextureSize()
        // {
        //     if (renderTexture != null)
        //     {
        //         renderTexture.Release(); // 古いリソースを解放
        //         renderTexture.width = Screen.width;
        //         renderTexture.height = Screen.height;
        //         renderTexture.Create();
        //     }
    }


    // Update is called once per frame
    void LateUpdate()
    {
        // 追随対象キャラが設定されている場合は、カメラをキャラの南側上方に設置する
        if (followTarget && followMode)
        {
            toCamera = new Marker(followTarget);
            Vector3 p = toCamera.position;
            toCamera.position = new Vector3(p.x, followCameraHeight, p.z - followCameraHeight * 1.0f);

            //toCamera.LookAt(followTarget);
            Vector3 dir = new Vector3(0.0f, -1.8f, 2.0f);//followTarget.position - toCamera.position;
            Quaternion rotation = Quaternion.LookRotation(dir);
            toCamera = new Marker(toCamera.position, rotation.eulerAngles);
        }

        var duration1 = 1.0f;
        var duration2 = 3.0f;

        if (elapsedTime < duration1 && fromCamera != null && toCamera != null)
        { // 自動視点移動中（高速）
            elapsedTime += Time.deltaTime;// * 1.0f;
            if (elapsedTime >= duration1) elapsedTime = duration1;
            mainCamera.transform.position = Vector3.Lerp(fromCamera.position, toCamera.position, elapsedTime / duration1);
            mainCamera.transform.localEulerAngles = AngleLerp(fromCamera.localEulerAngles, toCamera.localEulerAngles, elapsedTime);
            newAngle = mainCamera.transform.localEulerAngles;
        }
        else if (elapsedTime2 < duration2 && fromCamera != null && toCamera != null)
        { // 自動視点移動中（低速）
            elapsedTime2 += Time.deltaTime;// * 0.5f;
            if (elapsedTime2 >= duration2) elapsedTime2 = duration2;
            mainCamera.transform.position = Vector3.Lerp(fromCamera.position, toCamera.position, elapsedTime2 / duration2);
            mainCamera.transform.localEulerAngles = AngleLerp(fromCamera.localEulerAngles, toCamera.localEulerAngles, elapsedTime2);
            newAngle = mainCamera.transform.localEulerAngles;
        }
        else
        {  // カメラが自動移動しているときは操作不可
            if (followTarget && followMode)
            { // AutoTracking状態のときはターゲットに追随させる
                mainCamera.transform.position = new Vector3(
                    (toCamera.position.x + mainCamera.transform.position.x) * 0.5f,
                    (toCamera.position.y + mainCamera.transform.position.y) * 0.5f,
                    (toCamera.position.z + mainCamera.transform.position.z) * 0.5f
                );
                mainCamera.transform.localEulerAngles = toCamera.localEulerAngles;
            }
        }
        //コンパスの更新
        var fw = mainCamera.transform.forward;
        var rad = Mathf.Atan2(fw.x, fw.z);
        var deg = rad * 180 / Mathf.PI;
        //compass.transform.rotation = Quaternion.Euler(0.0f, 0.0f, deg);
        compass.transform.localEulerAngles = new Vector3(0, 0, deg);
    }

    // ある一点に向かって急速にカメラを寄せる
    public void ZoomTo()
    {
        Ray ray = Camera.main.ScreenPointToRay(lastTappedPos/*Input.mousePosition*/);
        // 平面を定義　原点を通るxz平面
        Plane plane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 20, 0));

        // レイと平面との当たり判定
        // ヒットした場合はenterに平面までの距離が格納される
        bool isHit = plane.Raycast(ray, out var enter);
        if (isHit)
        {
            Vector3 pt = ray.GetPoint(enter); // 平面との交点

            // ①カメラの位置から角度を変えずにズームインするやり方

            // Vector3 v = mainCamera.transform.position - pt; // 交点-->カメラのベクトル
            // v.y = 0; //xz平面に投影
            // if (v.x == 0 && v.z == 0) { // 万が一真上を指している場合は南側から見るように調整
            //     v.z = -1;
            // } 
            // v = pt + v.normalized * 80; // 80m離れたところから交点を見る
            // v.y = 30; // やや上方から見下ろす

            // ②画面の真下の方角から交点を見下ろすやり方
            Vector3 v = -1 * mainCamera.transform.forward; // カメラの角度の反対向きベクトル
            v.y = 0;
            v = pt + v.normalized * 50; // 80m離れたところから交点を見る
            v.y = 30; // やや上方から見下ろす

            fromCamera = new Marker(mainCamera.transform);

            //toCamera.LookAt(pt);
            Vector3 direction = pt - v;
            Quaternion rotation = Quaternion.LookRotation(direction);
            toCamera = new Marker(v, rotation.eulerAngles);
            //newAngle = toCamera.localEulerAngles;
            elapsedTime = 0.0f;
            ResetInertia();
        }
    }

    // コンパスボタンクリック時
    public void OnClickCompass()
    {
        if (elapsedTime < 1.0f) return;
        activateCameraButton(true);
        fromCamera = new Marker(mainCamera.transform);
        toCamera = new Marker(inCamera);
        elapsedTime = 0.0f;
        mouseMoveFlag = true;
        ResetInertia();
    }

    // カメラボタンクリック時
    public void OnClickCameraButton()
    {
        cameraButton.GetComponent<Button>().interactable = false;
        followMode = true;
        fromCamera.position = mainCamera.transform.position;
        fromCamera.localEulerAngles = mainCamera.transform.localEulerAngles;
        elapsedTime = 0.0f;
    }

    // カメラ追随ボタンクリック時
    public void activateCameraButton(bool flag)
    {
        cameraButton.GetComponent<Button>().interactable = flag;
        followMode = !flag; // 追随カメラがOFF ＝ 追随モード
    }

    // Forward/Backボタンタップ（使用しない）
    public void OnForwardButtonDown()
    {
        if (elapsedTime < 1.0f) return;
        inOperation = true;
        isForwardButtonDown = true;
        mouseMoveFlag = true;
        //activateCameraButton(true);
    }
    public void OnForwardButtonUp()
    {
        inOperation = false;
        isForwardButtonDown = false;
    }

    public void OnBackButtonDown()
    {
        if (elapsedTime < 1.0f) return;
        inOperation = true;
        isBackButtonDown = true;
        mouseMoveFlag = true;
        //activateCameraButton(true);
    }
    public void OnBackButtonUp()
    {
        inOperation = false;
        isBackButtonDown = false;
    }

    // 速度スライダー値更新
    public void OnChangeSpeedSlider()
    {
        // 値の変換: 0はそのまま1~100は10.6〜70.0
        moveSpeed = speedSlider.value * 2;// == 0 ? 0 : speedSlider.value * 0.6f + 20.0f;
        //Debug.Log(moveSpeed);
    }

    // 何らかのコントロール（ボタンやスライダーなど）のDown〜Upまでの期間はカメラ操作を無効にする
    public void OnDownControl()
    {
        inOperation = true;
        // コントロール操作開始時に慣性をリセット
        ResetInertia();
    }
    public void OnUpControl()
    {
        inOperation = false;
    }

    // 慣性をリセットするメソッド
    private void ResetInertia()
    {
        rotationInertia = Vector2.zero;
        panInertia = Vector2.zero;
        zoomInertia = 0f;
    }

    public void OnClickQuestion()
    {
        ToggleHelpView();
    }

    public void ToggleHelpView()
    {
        helpViewScrollRect.verticalNormalizedPosition = 1f;
        isHelpShown = !isHelpShown;
        if (isHelpShown)
        {
            helpViewTransform.anchoredPosition = new Vector2(0, -Screen.height);
            helpView.SetActive(true);
            helpViewTransform.DOAnchorPos(new Vector2(0, 0), 0.3f).SetEase(Ease.OutCubic);
        }
        else
        {
            helpViewTransform.DOAnchorPos(new Vector2(0, -Screen.height), 0.2f).SetEase(Ease.OutCubic).OnComplete(HideHelpView);
        }
    }

    private void HideHelpView()
    {
        helpView.SetActive(false);
    }


    // マップを選択
    public void SelectMap(string bookName)
    {
        if (elapsedTime < 1.0f) return;

        mouseMoveFlag = true;
        ResetInertia();

        // renderTextureCamera用
        fromCameraPre = new Marker(mainCamera.transform);
        toCameraPre = null;

        // mainCamera用
        fromCamera = null;
        toCamera = null;

        if (bookName == "Genesis" || bookName == "Acts")
        { // Wide Map
            nextMap = lv4Map;
            if (currentMap == lv6Map)
            { // 6 => 4
                toCameraPre = new Marker(currentMap.transform.Find("Camera6"));
                fromCamera = new Marker(nextMap.transform.Find("Camera46"));
            }
            else if (currentMap == lv7Map)
            { // 7 => 4
                toCameraPre = new Marker(currentMap.transform.Find("Camera7"));
                fromCamera = new Marker(nextMap.transform.Find("Camera47"));
            }
            toCamera = new Marker(nextMap.transform.Find("Camera4"));
        }
        else if (bookName == "Exodus")
        { // Middle Map
            nextMap = lv6Map;
            if (currentMap == lv4Map)
            { // 4 => 6
                toCameraPre = new Marker(currentMap.transform.Find("Camera46"));
                toCamera = new Marker(nextMap.transform.Find("Camera6"));
            }
            else if (currentMap == lv7Map)
            { // 7 => 6
                toCameraPre = new Marker(currentMap.transform.Find("Camera7"));
                fromCamera = new Marker(nextMap.transform.Find("Camera67"));
                toCamera = new Marker(nextMap.transform.Find("Camera6"));
            }
        }
        else if (Regex.IsMatch(bookName, "^(Joshua|Kings_Chronicles|Gospels)$"))
        { // Israel Map
            nextMap = lv7Map;
            if (currentMap == lv4Map)
            { // 4 => 7
                toCameraPre = new Marker(currentMap.transform.Find("Camera47"));
            }
            else if (currentMap == lv6Map)
            { // 6 => 7
                toCameraPre = new Marker(currentMap.transform.Find("Camera67"));
            }
            toCamera = new Marker(nextMap.transform.Find("Camera7"));

        }
        else
        {
            nextMap = null;
        }

        if (placeLabels)
        {
            placeLabels.SetActive(false);
        }
        if (nextMap)
        {
            placeLabels = nextMap.transform.Find("PlaceLabels").Find(bookName).gameObject;
            placeLabels.SetActive(true);
        }

        if (nextMap != currentMap)
        {
            EpisodeBase.ResetEpisode(); // エピソードプレイ中であればリセット

            switchMapTime = 0.0f;
            currentMap = nextMap;
            UpdateMapBounds();
            //inCamera = toCamera.Instanciate();
            //CopyTransform(inCamera, toCamera);

            inCamera = new Marker(toCamera);

            StartCoroutine("SwitchMap");
        }
    }

    // マップ切り替えフロー
    // 1.mainCameraの位置と角度をrenderTextureCameraにコピー
    // 2.renderTextureを表示
    // 3.mainCameraの位置と角度に切り替え後マップのInCamera（またはOutCamera）のものをコピー
    // 4. renderTextureCameraの位置と角度を切り替え前マップのOutCamera（またはInCamera）に徐々に近づける
    //    同時に徐々に透明度を上げていく
    //    透明度が100%になったらカメラの移動も終了し、renderTextureを非表示とする
    private IEnumerator SwitchMap()
    {
        var img = renderTexture.GetComponent<RawImage>();
        var startPos = fromCameraPre.position;
        var startAngle = fromCameraPre.localEulerAngles;
        var time = 0.0f;

        // RenderTextureオブジェクトのサイズを画面サイズに合わせる
        var length = Screen.height;//Mathf.Min(Screen.width, Screen.height);
        var rectTransform = renderTexture.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);

        // RenderTextureCameraの位置と向きをメインカメラの位置と向きに
        renderTextureCamera.transform.position = startPos;
        renderTextureCamera.transform.localEulerAngles = startAngle;

        mainCamera.transform.position = fromCamera != null ? fromCamera.position : toCamera.position;
        mainCamera.transform.localEulerAngles = fromCamera != null ? fromCamera.localEulerAngles : toCamera.localEulerAngles;

        // RenderTextureの透明度を0に戻し、表示する
        renderTexture.GetComponent<RawImage>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        renderTexture.SetActive(true);

        if (fromCamera != null)
        {
            Invoke("MoveCamera2", 0.1f);
        }

        //for (float alpha = 1.0f ; alpha >= 0.0f ; alpha -= 0.04f){
        float alpha = 1.0f;
        while (true)
        {
            img.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            if (time < 1.0f)
            {
                renderTextureCamera.transform.position = Vector3.Lerp(startPos, toCameraPre.position, time);
                renderTextureCamera.transform.localEulerAngles = AngleLerp(startAngle, toCameraPre.localEulerAngles, time);
                time += 0.05f;
            }
            if (alpha == 0.0f) break;
            //if (time > 0.1f) {
            alpha = Mathf.Max(alpha - 0.05f, 0.0f);
            //}

            yield return new WaitForSeconds(0.05f);
        }
        renderTexture.SetActive(false);
        startCameraMarker = toCamera;
        yield return null;
    }

    private void MoveCamera2()
    {
        elapsedTime = 0.0f;
    }

    // 角度Lerp（変化量が最小になるように最適化）
    private Vector3 AngleLerp(Vector3 from, Vector3 to, float t)
    {
        // 各軸ごとに最小回転角を計算
        return new Vector3(
            Mathf.LerpAngle(from.x, to.x, t),
            Mathf.LerpAngle(from.y, to.y, t),
            Mathf.LerpAngle(from.z, to.z, t)
        );
    }


    // エピソードの開始
    public IEnumerator PlayEpisode(string episodeName)
    {
        activateCameraButton(false); // 追随モードON
        cameraButton.SetActive(true);
        speedSliderPanel.SetActive(true);
        followCameraHeight = 60.0f; // カメラ位置をリセット

        if (episodeName == "Paul'sJourney1")
        {
            yield return StartCoroutine(new ActsJourney1().Play());
        }
        else if (episodeName == "Paul'sJourney2")
        {
            yield return StartCoroutine(new ActsJourney2().Play());
        }
        else if (episodeName == "Paul'sJourney3")
        {
            yield return StartCoroutine(new ActsJourney3().Play());
        }
        else if (episodeName == "WildJourney")
        {
            yield return StartCoroutine(new Exodus().Play());
        }
        ExitEpisode();
    }

    // エピソードプレイ完了時に視点を上に
    private void ExitEpisode()
    {
        elapsedTime2 = 0.0f;
        fromCamera = new Marker(mainCamera.transform);
        //toCamera = startCameraMarker;
        // 少しだけ引いた位置へカメラを移動
        var toPos = mainCamera.transform.position - mainCamera.transform.forward * 400.0f;
        toCamera = new Marker(toPos, mainCamera.transform.eulerAngles);
        activateCameraButton(true);
        cameraButton.SetActive(false);
        //ResetEpisode();
    }

    public void ResetEpisode()
    {
        activateCameraButton(true);
        cameraButton.SetActive(false);
        speedSliderPanel.SetActive(false);
        EpisodeBase.ResetEpisode();
    }

}
