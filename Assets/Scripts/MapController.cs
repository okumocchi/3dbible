using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private MeshRenderer mesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDisable() {
        mesh = GetComponent<MeshRenderer>();
        StartCoroutine("FadeOut");
    }
 
    private IEnumerator FadeOut()
    {
        for ( int i = 0 ; i < 255 ; i++ ){
            mesh.material.color = mesh.material.color - new Color32(0,0,0,1);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
