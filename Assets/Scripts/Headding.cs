using UnityEngine;

public class Headding : MonoBehaviour
{
    private float fromY;
    private float toY;
    private float t = 99999;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float duration = 0.5f;
        if (t < duration) {            
            t += Time.deltaTime;
            if (t > duration) {
                t = duration;
            }
            float y = Easing.ElasticOut(t, duration, fromY, toY);
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }
        
        
    }

    public void SetPositions(float _fromY, float _toY) {
        fromY = _fromY;
        toY = _toY;
        t = 0;
    }

    public void OnClickBookButton() {
        transform.parent.gameObject.GetComponent<HeaddingsController>().SelectBook(transform);
    }

    public void OnClickEpisodeButton() {
        transform.parent.parent.gameObject.GetComponent<HeaddingsController>().SelectEpisode(transform);
    }
}
