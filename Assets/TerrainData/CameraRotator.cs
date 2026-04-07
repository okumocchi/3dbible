using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    // カメラオブジェクトを格納する変数
    public Camera mainCamera;
    // カメラの回転速度を格納する変数
    public Vector2 rotationSpeed;
    // マウス移動方向とカメラ回転方向を反転する判定フラグ
    public bool reverse;

    public GameObject compass;

    // マウス座標を格納する変数
    private Vector2 lastMousePosition;
    // カメラの角度を格納する変数（初期値に0,0を代入）
    private Vector2 newAngle = new Vector2(0, 0);


    void Start() {
                    // カメラの角度を変数"newAngle"に格納
            newAngle = mainCamera.transform.localEulerAngles;

    }

    // ゲーム実行中の繰り返し処理
    void Update()
    {
        float moveSpeed = 120.0f;
        float lowLimit = 12f;
        float highLimit = 600f;
        var pos = mainCamera.transform.position;
        //Wキーがおされたら
        if(Input.GetKey(KeyCode.W))
        {
            pos += mainCamera.transform.forward * moveSpeed * Time.deltaTime;
            if (pos.y < lowLimit) pos.y = lowLimit;
            else if (pos.y > highLimit) pos.y = highLimit;
            mainCamera.transform.position = pos;
			//mainCamera.transform.position += mainCamera.transform.forward * moveSpeed * Time.deltaTime;
        }
        //Sキーがおされたら
        if (Input.GetKey (KeyCode.S))
		{
            pos += mainCamera.transform.forward * -1.0f * moveSpeed * Time.deltaTime;
            if (pos.y < lowLimit) pos.y = lowLimit;
            else if (pos.y > highLimit) pos.y = highLimit;
            mainCamera.transform.position = pos;
			//mainCamera.transform.position += mainCamera.transform.forward * -1.0f * moveSpeed * Time.deltaTime;
		}

        // 左クリックした時
        if (Input.GetMouseButtonDown(0))
        {
            // カメラの角度を変数"newAngle"に格納
            //newAngle = mainCamera.transform.localEulerAngles;
            //Debug.Log("Click!");
            //                        Debug.Log(newAngle.x + " " + newAngle.y);

            // マウス座標を変数"lastMousePosition"に格納
            lastMousePosition = Input.mousePosition;
        }
        // 左ドラッグしている間
        else if (Input.GetMouseButton(0))
        {
            //カメラ回転方向の判定フラグが"true"の場合
            if (!reverse)
            {
                // Y軸の回転：マウスドラッグ方向に視点回転
                // マウスの水平移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.y -= (lastMousePosition.x - Input.mousePosition.x) * rotationSpeed.y;
                // X軸の回転：マウスドラッグ方向に視点回転
                // マウスの垂直移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.x -= (Input.mousePosition.y - lastMousePosition.y) * rotationSpeed.x;

                        //Debug.Log(newAngle.x + " " + newAngle.y);

                if (newAngle.x < 340) {
                    newAngle.x  = 340;
                } else if (newAngle.x > 450) {
                    newAngle.x  = 450;
                }
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;

                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
            // カメラ回転方向の判定フラグが"reverse"の場合
            else if (reverse)
            {
                // Y軸の回転：マウスドラッグと逆方向に視点回転
                newAngle.y -= (Input.mousePosition.x - lastMousePosition.x) * rotationSpeed.y;
                // X軸の回転：マウスドラッグと逆方向に視点回転
                newAngle.x -= (lastMousePosition.y - Input.mousePosition.y) * rotationSpeed.x;
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;
                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
        }

        var fw = mainCamera.transform.forward;
        var rad = Mathf.Atan2(fw.x, fw.z);
        //Debug.Log(fw.y);
        var deg = rad * 180 / Mathf.PI;
        //compass.transform.rotation = Quaternion.Euler(0.0f, 0.0f, deg);
        compass.transform.localEulerAngles = new Vector3(0, 0, deg);
        //var fw = compass.transform.forward;
        //Debug.Log(fw.x + " " + fw.y + " " + fw.z);
    }

    // マウスドラッグ方向と視点回転方向を反転する処理
    public void DirectionChange()
    {
        // 判定フラグ変数"reverse"が"false"であれば
        if (!reverse)
        {
            // 判定フラグ変数"reverse"に"true"を代入
            reverse = true;
        }
        // でなければ（判定フラグ変数"reverse"が"true"であれば）
        else
        {
            // 判定フラグ変数"reverse"に"false"を代入
            reverse = false;
        }
    }
}