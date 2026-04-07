using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleController : MonoBehaviour
{
    private Vector3 orgPosition;
    private Vector3 orgAngles;
    public List<GameObject> crews = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {  
        orgPosition = transform.position;
        orgAngles = transform.localEulerAngles;             
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetTransform() {
        transform.position = orgPosition;
        transform.localEulerAngles = orgAngles;
        crews.Clear();
    }

    public void GetOn(CharaController chara) {
        crews.Add(chara.gameObject);
        //Debug.Log("GetOn:" + crews.Count);
    }

    public void GetOff(CharaController chara) {
        crews.Remove(chara.gameObject);
        //Debug.Log("GetOff:" + crews.Count);
    }

    // 乗るべき全員が乗り込んだかどうか
    public bool IsReadyToGo() {
        bool isAllOn = true;
        foreach(GameObject crew in crews) {   
            var followers = crew.GetComponent<CharaController>().Followers;
            //Debug.Log(crew.GetComponent<CharaController>().Name + "のfollowersをチェック:" + followers.Count); 
            foreach(GameObject follower in followers) {
                //Debug.Log(follower.GetComponent<CharaController>().Name + "は乗っている？");
        
                if (!crews.Contains(follower)) {
                    //Debug.Log(follower.GetComponent<CharaController>().Name + "がまだ乗っていません！");
                    isAllOn = false;
                    break;
                }
            }
            if (!isAllOn) break;
        }
        return isAllOn;
    }
}
