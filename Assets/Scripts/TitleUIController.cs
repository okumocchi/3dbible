using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class TitleUIController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform target;
    [SerializeField] private GameObject renderTexture;
    [SerializeField] private GameObject message;

    



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTouchCanvas() {
        if (message.activeSelf) {
            message.SetActive(false);
            StartCoroutine(MoveCamera(mainCamera, target, 2.0f));
            StartCoroutine("FadeMap");
        }
        
        
    }

    private IEnumerator MoveCamera(Camera camera, Transform target, float duration) {
        float interval = 0.02f; 
        Vector3 startPos = camera.transform.position;
        Vector3 startAngle = camera.transform.eulerAngles;

        for (float t = 0.0f; t < duration; t += interval) {
            camera.transform.position =  Vector3.Lerp(startPos, target.position, t / duration);
            //camera.transform.localEulerAngles = Vector3.Lerp(startAngle, target.localEulerAngles, t / duration);

            // 上の角度Lerpは変化量を最小にする機能はないので改良
            Vector3 angles = startAngle;
            // 各軸ごとに最小回転角を計算
            angles.x = Mathf.LerpAngle(startAngle.x, target.eulerAngles.x,  t / duration);
            angles.y = Mathf.LerpAngle(startAngle.y, target.eulerAngles.y,  t / duration);
            angles.z = Mathf.LerpAngle(startAngle.z, target.eulerAngles.z,  t / duration);
            camera.transform.localEulerAngles = angles;

            yield return new WaitForSeconds(interval);
        }

        yield return null;
    }

    private IEnumerator FadeMap() {
        // renderTextureのサイズを画面と一致させる
        var length = Screen.height;
        var rectTransform = renderTexture.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);

        var img = renderTexture.GetComponent<RawImage>();    
        var time = 0.0f;

        yield return new WaitForSeconds(3f);

        for (float alpha = 0.0f ; alpha <= 1.0f ; alpha += 0.025f){
            img.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            yield return new WaitForSeconds(0.05f);
        }
        SceneManager.LoadScene("WideMap");
        yield return null;
    }


    void ChangeScene() {
        SceneManager.LoadScene("WideMap");
    }
}
