using UnityEngine;

public class Globe : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, -1.5f * Time.deltaTime, 0);
        
    }
}
