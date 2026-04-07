using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Splines;

public class EpisodeBase 
{
    static protected GameObject lv4Map;
    static protected GameObject lv6Map;
    static protected GameObject lv7Map;
    static protected GameObject humanPrefab;
    static public Action<Transform> SetFollowTarget;

    static protected List<GameObject> castList = new List<GameObject>();
    static protected GameObject currentMap;
    static protected string bookName;

    static private List<Coroutine> coroutineList = new List<Coroutine>();

    static public void SetObjects(GameObject map4, GameObject map6, GameObject map7, GameObject human) {
        lv4Map = map4;
        lv6Map = map6;
        lv7Map = map7;
        humanPrefab = human;
    }

    static public Coroutine AsyncPlay(IEnumerator coroutine) {
        return CoroutineRunner.Start(coroutine);
    }

    static public void SetMapAndBook(GameObject map, string book) {
        currentMap = map;
        bookName = book; 
        ResetEpisode(); // ここでリセット
    }

    static public GameObject FindVehicle(string name) {
         return currentMap.transform.Find("PlaceLabels").Find(bookName).Find(name).gameObject; 
    }

    static public GameObject Spawn(Vector3 pos) {
        var cast = GameObject.Instantiate(humanPrefab);
        cast.transform.position = pos;
        castList.Add(cast);
        return cast;
    }

    static public CharaController Charactorize(GameObject cast, string labelKey) {
        var chara = cast.GetComponent<CharaController>();
        //if (labelKey != "") {
            chara.SetLabel(labelKey);
            //chara.Name = labelKey;
        //}
        chara.SetMap(currentMap);
        return chara;
    }

    public Mobs SpawnMobs(int n, Vector3 pos, string label) {
        var mobs = new Mobs(label);
        var head = Spawn(pos);
        head.GetComponent<CharaController>().IsMob = true;
        var followees = new Queue<GameObject>();
        Charactorize(head, "");
        mobs.AddMob(head);
        mobs.Head = head;
            //var preMob = head;
        followees.Enqueue(head);
        int midN = (int)(n / 2);
        int followerCount = 0;
        GameObject followee = null;
        while (--n > 0) {
            if (followerCount == 0) {
                followee = followees.Dequeue();
            }
            followerCount++;            
            var mob = Spawn(pos);
            mobs.AddMob(mob);
            var charMob = Charactorize(mob, n == midN ? label : ""); // 中央のキャラの上にのみラベルを付与
            charMob.IsMob = true;
            charMob.SetFollowee(followee);
            followees.Enqueue(mob);
            if (followerCount >= 2) { // 一人につき2人のフォロワをつける
                followerCount = 0;
            }
            //preMob = mob;
        }

        return mobs;
    }

    static public void ResetEpisode() {
        CoroutineRunner.StopRunningCoroutines();
        foreach(var cast in castList) {
            GameObject.Destroy(cast);
        }
        castList.Clear();

        if (currentMap) {
            var vehicleList = currentMap.transform.Find("PlaceLabels").Find(bookName).GetComponentsInChildren<VehicleController>();
            foreach(var vehicle in vehicleList) {
                vehicle.ResetTransform();
            }
        }
    }

    // キャスト（1人）退場
    static public void CastOut(GameObject cast) {
        castList.Remove(cast);
        GameObject.Destroy(cast);
    }

    // キャスト（複数）退場
    static public void CastsOut(List<GameObject> casts) {
        foreach(var cast in casts) {
            CastOut(cast);
        }
    }

    static public void SwitchPath(CinemachineSplineCart cart, SplineContainer newPath, float pos)  {
        cart.Spline = null;
        cart.SplinePosition = pos;
        cart.Spline = newPath;
    }

    static public IEnumerator ResetVehicle(Transform v, float delay) {
        yield return WaitForAWhile(delay);
        v.GetComponent<VehicleController>().ResetTransform();
    }

    // 退場
    static public void Leave(GameObject cast) {
        castList.Remove(cast);
        cast.GetComponent<CharaController>().Leave();
    }


    static public IEnumerator WaitForAWhile(float sec) {
        //  デフォルトのmoveSpeed62のときに指定された秒数待つ
        yield return new WaitForSeconds(62 * (1.0f / UIController.moveSpeed) * sec);
    }

    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj); // シーン遷移時に破棄されないようにする
                }
                return instance;
            }
        }

        public static Coroutine Start(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }

        public static void StopRunningCoroutines() {
            Instance.StopAllCoroutines();
        }
    }




}

public class Mobs {
    private List<GameObject> mobs;
    private GameObject head;
    public GameObject Head {set; get;}
    private string label;

    public Mobs(string label) {
        mobs = new List<GameObject>();
        this.label = label;
    }
    public void AddMob(GameObject mob) {
        //mob.GetComponent<CharaController>().SetLabel(label);
        mobs.Add(mob);
    }

}


