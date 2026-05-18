using UnityEngine;
using TMPro;
using System.Collections;



// 地図上のラベルが常に視点を向くように制御する
// さらに大きさが適切になるように調整する
public class DirectionController : MonoBehaviour
{
    public GameObject target; // mainCamera
    private Transform focusTr; // focusしたときのカメラ位置角度

    void Start()
    {
    }

    void LateUpdate()
    {
        //transform.LookAt(target.transform);
         transform.forward = Camera.main.transform.forward;
         float dist = Vector3.Distance(transform.position, target.transform.position);
         
         // 一定以上、以下のサイズにならないように調整
         //float scale = Mathf.Max(1.2f, Mathf.Min(2.0f, dist / 100.0f));
         float scale = Mathf.Max(0.6f,dist / 200.0f);
         transform.localScale = new Vector3(scale, scale, scale); 
         //var tmp = GetComponent<TextMeshProUGUI>();
         //tmp.fontSize = 100


    }

    // // ラベルがタップされたらそこにフォーカスする
    // public void OnClick() {
    //     Debug.Log("hoge!");
    //     Vector3 pos = transform.position;
    //     focusTr.position = new Vector3 (pos.x, pos.y + 50, pos.z - 50);
    //     focusTr.LookAt(transform.position);
    //     StartCoroutine(Focus(target, focusTr, 1.0f));


    // }

    // private IEnumerator Focus(GameObject camera, Transform target, float duration) {
    //     float interval = 0.02f; 
    //     Vector3 startPos = camera.transform.position;
    //     Vector3 startAngle = camera.transform.eulerAngles;

    //     for (float t = 0.0f; t < duration; t += interval) {
    //         camera.transform.position =  Vector3.Lerp(startPos, target.position, t / duration);
    //         //camera.transform.localEulerAngles = Vector3.Lerp(startAngle, target.localEulerAngles, t / duration);

    //         // 上の角度Lerpは変化量を最小にする機能はないので改良
    //         Vector3 angles = startAngle;
    //         // 各軸ごとに最小回転角を計算
    //         angles.x = Mathf.LerpAngle(startAngle.x, target.eulerAngles.x,  t / duration);
    //         angles.y = Mathf.LerpAngle(startAngle.y, target.eulerAngles.y,  t / duration);
    //         angles.z = Mathf.LerpAngle(startAngle.z, target.eulerAngles.z,  t / duration);
    //         camera.transform.localEulerAngles = angles;

    //         yield return new WaitForSeconds(interval);
    //     }

    //     yield return null;
    // }
}