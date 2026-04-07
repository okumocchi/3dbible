using UnityEngine;
using System.Collections;

public class TitleCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("Rotate");   
    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.Rotate(10f * Time.deltaTime, -30f * Time.deltaTime, 20f * Time.deltaTime);
        
    }

    IEnumerator Rotate() {
        int count = 0;
        while (true) {

            yield return new WaitForSeconds(3.0f);
            float duration = 1.5f;
            float t = 0.0f;
            while (t < duration) {
                t += 0.05f;
                float angle = Easing.CubicInOut(t, duration, 0, 90);
                //this.transform.Rotate(0, angle, 0);
                float angleX = count % 2 == 0 ? angle : 0;
                float angleY = -1 * angle;
                float angleZ = count % 3 == 0 ? angle : 0;

                this.transform.localEulerAngles = new Vector3(angleX, angleY, angleZ);
                yield return new WaitForSeconds(0.05f);
            }
            count++;
        }
        yield return null;
    }

}
