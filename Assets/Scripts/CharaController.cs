using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public enum States {
    None, // Freezed状態
    Idle,
    Walk,
    WalkCircle,
    Wander, // 行ったり来たりする
    Stroll, // ちょっと歩いては立ち止まるを繰り返す（時間指定なし）
    Vehicle,
    Leaving, // いなくなる（死ぬ）
}


public class CharaController : MonoBehaviour
{
    [SerializeField] Material humanMaterialTransparent;

    private float curPos = 0.0f; // 現在設定されているパス上の位置（時間配分）
    //private float toPos = 0.0f;
    //private Transform vehicle = null; // 現在乗っている乗り物
    private GameObject Vehicle { get; set;} 
    private Terrain land = null;
    private Terrain sea = null;

    private GameObject followee = null; // フォローする人
    private List<GameObject> followers = new List<GameObject>();
    public List<GameObject> Followers {
        get {return followers;}
    }


    public States State { get; set;}

    private float  REFRESH_TIME = 0.017f; //およそ1/60秒

    private int rideWaitCount = 0; // followeeが乗り込んでから間をおいて乗り込む

    private float blurAngle; // 左右のぶれ（Y軸回転角度）
    
    // 次に進むべきposについてxzそれぞれブレを入れる範囲
    private float blurX;
    private float blurZ;
    
    //モブがある程度ばらけるようにランダムに目的地をずらす
    private Vector3 destPosition;

    //mobかどうか
    private bool isMob = false;
    public bool IsMob {get; set;}

    private Tracks tracks;

    public string Name {get; set;}
    private float waitTime = 0.0f; // 次のupdate処理までの待ち時間

    private Vector3 orgPos; // Strollの歳の基点
    private Vector3 strollPos; // Stroll行動のときに次に向かう位置
    private int strollIdleTime = 0; // Stroll中の立ち止まり時間

    private float alpha = 1.0f;

    //private GameObject orbit = new GameObject(); // 先頭の際に先導するオブジェクト

    void Start()
    {
        tracks = new Tracks(5); // 10フレーム前までの位置と向きを保持しておく
        Vehicle = null;
        //State = States.Walk;
        gameObject.GetComponent<Animator>().SetBool("walk", true); // とりあえず常に歩きアニメーション   
        blurAngle = 0.0f; // test
        tracks.AddTrack(transform); // 足跡を保存 

        blurX = Random.Range(-1, 1);
        blurZ = Random.Range(-1, 1);
        destPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (!land) return;

        if(UIController.moveSpeed == 0) return;

        waitTime += Time.deltaTime;
        float lerpRate = waitTime >= 1.0f / UIController.moveSpeed  ? 1.0f :
             waitTime / 1.0f / UIController.moveSpeed;

        var followeeChara = followee ? followee.GetComponent<CharaController>(): null;
        var followeeVehicle = followeeChara ? followeeChara.Vehicle : null;

        if (State == States.Leaving) {
            var pos = transform.position;
            pos.y += 0.4f;
            transform.position = pos;
            alpha = Mathf.Max(alpha - 0.02f, 0.0f);
            SetAlpha(gameObject, alpha);
            //var material = gameObject.transform.GetComponent<Renderer>().material;
		    //var color = material.color;
    		//material.color = new Color(color.r, color.g, color.b, alpha);

            if (alpha == 0.0f) {
                GameObject.Destroy(gameObject);
            }
        }
        else if (State == States.Stroll) { // Strollの場合は各自バラバラに動く
            var exForward = transform.forward;
            if (strollIdleTime == 1) {
                SetStrollPosition();
                strollIdleTime = 0;
            }  
            if (strollIdleTime == 0)  {
                LookAtH(transform, strollPos);
                if (Vector3.Angle(exForward, transform.forward) >= 120) {
                    strollIdleTime = Random.Range(30, 50);
                    //SetStrollPosition();
                } else {
                    transform.position += transform.forward * UIController.moveSpeed * 0.008f;    
                    StandOnTheGround(); // 地面の上に立つよう位置調整
                    float dist = Vector3.Distance(transform.position, strollPos);
                    if (dist < 1.0f) strollIdleTime = Random.Range(1, 50);//SetStrollPosition();
                }
            } else {
                strollIdleTime -= 1;
            }
        } else {
            // 先頭でパスに沿って移動している場合は毎フレーム位置の更新処理を入れる
            if (IsMovingOnPath()) {
                //var cart = GetComponent<CinemachineSplineCart>();
                //cart.SplinePosition = curPos;
                StandOnTheGround(); // 地面の上に立つよう位置調整
                if (State == States.Vehicle) {
                    var pos = transform.position;
                    //pos.y += 0.3f; // 船を少し持ち上げる（水が船内に入っているように見せないため）
                    Vehicle.transform.position = pos;
                    Vehicle.transform.localEulerAngles = transform.localEulerAngles;                   
                }   
            } else if (followeeVehicle != null && State == States.Vehicle) {
                transform.position = followee.transform.position;
            } else if(followee != null) { // 誰かの後を追う
                float dist = Vector3.Distance(transform.position, followee.transform.position);
                if (Vehicle) {
                    if (!followeeVehicle) { // 先頭がすでに下船している
                        if (dist > 3.0f || rideWaitCount++ >30) {
                            GetOff();
                            rideWaitCount = 0;
                        }
                    } 
                } else {
                    if (followeeVehicle != null && (dist <= 2.0f || rideWaitCount++ > 20)) { // 先頭が乗船している
                        JustGetOn(followeeVehicle);
                        rideWaitCount = 0;
                    } else if (State != States.Idle) {
                        if (IsMob) {
                            if(destPosition.x == 0 || Random.Range(0, 5) == 0) {
                                Vector3 fPos = followeeChara.transform.position;
                                if (dist > 10.0f) {
                                    destPosition = fPos;
                                } else{
                                    destPosition = new Vector3 (
                                        fPos.x + Random.Range(-3, 3),
                                        fPos.y,
                                        fPos.z + Random.Range(-3, 3)
                                    );
                                }
                            }
                            LookAtH(transform, destPosition); 
                            transform.position += transform.forward * UIController.moveSpeed * 0.009f; 
                            StandOnTheGround(); // 地面の上に立つよう位置調整
                        } else {
                            var track = followeeChara.GetOldestTrack();
                            if (track != null) {
                                Vector3 trackPos = BlurPosition(track.Position); // 機会的にならないように足跡の位置にブレを入れる
                                //transform.position = track.Position;
                                //LookAtH(transform, followee.transform.position); // Followeeの方を向く 
                                LookAtH(transform, trackPos); // Followeeの足跡を向く
                                //transform.LookAt(track.Position);
                                transform.position += transform.forward * UIController.moveSpeed * 0.008f; 
                                StandOnTheGround(); // 地面の上に立つよう位置調整

                            }
                            if (dist < 1.0f) {
                                if (/*followeeChara.State == States.Idle 
                                ||*/ followeeChara.State == States.Stroll) {
                                    //State = followeeChara.State;
                                    Stroll();
                                }
                            }
                            if (Random.Range(0, 9) == 0) {
                                blurX = Random.Range(-1, 1);
                                blurZ = Random.Range(-1, 1);
                            }                            
                        }


                    }
                }
            } else {
                StandOnTheGround();
            }
        }

        if (lerpRate == 1.0f) {  // 待ち時間が満ちたときは次の移動先座標を更新
            waitTime = 0.0f;
            tracks.AddTrack(transform); // 足跡を保存 
        }
    }

    // 死んだりいなくなったり
    public void Leave(){
        SetState(States.Leaving);
        gameObject.GetComponent<Animator>().SetBool("walk", false);
        // Materialをtransparetに変更する
        //Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        Renderer renderer = gameObject.transform.Find("Man").gameObject.GetComponent<Renderer>();
        //foreach (Renderer renderer in renderers) {
            renderer.material = humanMaterialTransparent;
        //}

        
    }


    public static void SetAlpha(GameObject go, float alpha) {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            if (!mat.HasProperty("_Color")) continue;

            // //mat.shader = Shader.Find("Standard");
            // mat.SetFloat("_Mode", 3); // Fade
            // // デフォルトのレンダータイプタグを 'Transparent' に上書き
            // mat.SetOverrideTag("RenderType", "Transparent");
            
            // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // mat.SetInt("_ZWrite", 0);
            // mat.DisableKeyword("_ALPHATEST_ON");
            // mat.EnableKeyword("_ALPHABLEND_ON");
            // mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // mat.renderQueue = 3000;

            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            renderer.material = mat;
        }
    }


    private Vector3 BlurPosition(Vector3 pos) {
        return new Vector3 (
            pos.x + blurX,
            pos.y,
            pos.z + blurZ
        );
    }
    // 地面（Terrain地形）の上に立つようにy座標を調整する
    private void StandOnTheGround() {
        Vector3 pos = transform.position;
        pos.y = GetTerrainHeight(pos, false);
        transform.position = pos;  
    }

    // 自分とフォロワに対してStateを設定
    public void SetState(States _state, bool anyStateFlag=false) {
        State = _state;
        if (_state == States.Stroll){ 
            SetStrollInfo();
        }
        if (anyStateFlag || State != States.Stroll && State != States.Idle && State != States.Vehicle) {
            foreach (var follower in followers) {
                follower.GetComponent<CharaController>().SetState(_state, anyStateFlag); // 再起的に全てのフォロワに設定
            }
        }
    }

    // フォロワの追加
    public void AddFollower(GameObject who) {
        followers.Add(who);
        //Debug.Log(Name + "のフォロワを追加しました:" + (who.GetComponent<CharaController>().Name) + followers.Count);
    }
    
    // フォロワの削除
    public void RemoveFollower(GameObject who) {
        //.Log(Name + "のフォロワを削除します:" + followers.Count);
        followers.Remove(who);
        //Debug.Log(Name + "のフォロワを削除しました:" + followers.Count);
    }

    // 歩いているか
    public bool IsWalking() {
        return State == States.Walk || State == States.WalkCircle || State == States.Wander;
    }

    // パスに沿って移動をしているか（先頭のみ）
    public bool IsMovingOnPath() {
        return followee == null && (State == States.Walk || State == States.Vehicle);
    }

    public void SetMap(GameObject map) {
        land = map.transform.Find("Terrain").gameObject.GetComponent<Terrain>();
        sea = map.transform.Find("Water").gameObject.GetComponent<Terrain>();
    }
    public void SetLabel(string stringKey) {
        var label = GetComponentInChildren<TextMeshPro>();
        var text = stringKey == "" ? "" :
            LocalizationSettings.StringDatabase.GetTableEntry("Persons", stringKey).Entry.Value;
        label.text = text;
    }

    // 一点から離れないようにフラフラする
    public void Stroll() {
        SetState(States.Stroll);
    }
    private void SetStrollInfo() {
        orgPos = transform.position;
        SetStrollPosition();
        strollIdleTime = 0;
    }

    public void SetStrollPosition() {
        strollPos = new Vector3(
            orgPos.x + Random.Range(-3.5f, 3.5f),
            orgPos.y,
            orgPos.z + Random.Range(-3.5f, 3.5f)
        );
    }

    // 一点を中心に行ったり来たりする
    public IEnumerator Wander(float duration) {
        SetState(States.Wander);
        float elapsedTime = 0.0f;
        float turnTime = 0.0f;

        // 自由に動けるようにパスを一旦外す
        var cart = GetComponent<CinemachineSplineCart>();
        var path = cart.Spline;
        cart.Spline = null;

        Vector3 orgPos = transform.position;

        var anim = gameObject.GetComponent<Animator>();
        var _duration = UIController.moveSpeed == 0 ? 9999.0f : 1.0f / UIController.moveSpeed * duration *25.0f;
        float oneWayDuration = UIController.moveSpeed == 0 ? 1.0f : 1.0f / UIController.moveSpeed * 4.0f;
        while (elapsedTime < _duration) {
            _duration = UIController.moveSpeed == 0 ? 9999.0f : 1.0f / UIController.moveSpeed * duration * 25.0f;
            oneWayDuration = UIController.moveSpeed == 0 ? 1.0f : 1.0f / UIController.moveSpeed * 4.0f;
            if (UIController.moveSpeed == 0) {
                yield return new WaitForSeconds(1.0f);
            } else {
                Vector3 course = new Vector3(0, Random.Range(90.0f, 150.0f) * (Random.value < 0.5f ? -1 : 1), 0);
                transform.Rotate(course);
                turnTime = 0.0f;
                while (turnTime < oneWayDuration) {
                    transform.position += transform.forward * /*Time.deltaTime * 40.0f;*/ UIController.moveSpeed * 0.008f;
                    turnTime += Time.deltaTime;
                    elapsedTime += Time.deltaTime;
                    yield return null;//new WaitForSeconds(1.0f / UIController.moveSpeed * 3.0f);                   
                }
                LookAtH(transform, orgPos);
                turnTime = 0.0f;
                while (turnTime < oneWayDuration) {
                    transform.position += transform.forward * /*Time.deltaTime * 40.0f;*/UIController.moveSpeed * 0.008f;
                    turnTime += Time.deltaTime;
                    elapsedTime += Time.deltaTime;
                    yield return null;//new WaitForSeconds(1.0f / UIController.moveSpeed * 3.0f) ;                   
                }
            }
        }

        // パスを復帰
        cart.Spline = path;
        //anim.SetBool("walk", false);
    }

    // １点を中心に円を描いて歩き回る
    public IEnumerator WalkCircle(Vector3 center, float radius, float duration) {
        State = States.WalkCircle;
        float elapsedTime = 0.0f;
        // 自由に動けるようにパスを一旦外す
        var cart = GetComponent<CinemachineSplineCart>();
        var path = cart.Spline;
        cart.Spline = null;

        var anim = gameObject.GetComponent<Animator>();
        //anim.SetBool("walk", true);        

        Vector3 orbit = center;
        float angularSpeed = Random.value < 0.5 ? -1 : 1;// 回転速度（ラジアン/秒）        
        float angle = Random.Range(0f, 360f);// 現在の角度（ラジアン）
        while (elapsedTime < duration) {
            //fromPosition = transform.position;
            //toPosition = null;

            if (UIController.moveSpeed == 0.0f) {
                yield return new WaitForSeconds(1.0f);
            } else {
                angle += /*Time.deltaTime*/  angularSpeed * UIController.moveSpeed * 0.001f;
                // 新しい位置を計算
                orbit.x = center.x + radius * Mathf.Cos(angle);
                orbit.z = center.z + radius * Mathf.Sin(angle);

                LookAtH(transform, orbit);
                //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                transform.position += transform.forward * /*Time.deltaTime * 20.0f;*/ UIController.moveSpeed * 0.008f; 
                StandOnTheGround();
                //toPosition = transform.position + transform.forward * Time.deltaTime * 20.0f;                    

                elapsedTime += Time.deltaTime;//REFRESH_TIME;
                yield return null;//new WaitForSeconds(1.0f / UIController.moveSpeed);
            }
        }
        // パスを復帰
        cart.Spline = path;
        //anim.SetBool("walk", false);        

    }

    // パスに沿って移動（徒歩か乗り物）
    public IEnumerator Move(float toPos) {
        var cart = GetComponent<CinemachineSplineCart>();
        while(curPos < toPos) {
            if (UIController.moveSpeed == 0.0f) {
                yield return new WaitForSeconds(1.0f);
            } else {
                curPos += UIController.moveSpeed * 0.008f;//0.8f;//Time.deltaTime * ;
                if (/*speed > 0 &&*/ curPos >= toPos /*|| speed < 0 && curPos <= toPos*/) {
                    curPos = toPos;
                }
                cart.SplinePosition = curPos;

                yield return null;//new WaitForSeconds(/*REFRESH_TIME*/ 1.0f / UIController.moveSpeed);  // 60fps
            }
        }
        if (State != States.Vehicle) State = States.Idle;
    }

    public IEnumerator Move(float fromPos, float toPos) {
        curPos = fromPos;
        yield return StartCoroutine(Move(toPos));        
    }

    // パスに沿って歩く（先頭キャラにのみ適用）
    public IEnumerator Walk(float toPos) {
        //State = States.Walk;
        SetState(States.Walk);
        yield return StartCoroutine(Move(toPos));
    }

    public IEnumerator Walk(float fromPos, float toPos) {
        curPos = fromPos;
        yield return StartCoroutine(Walk(toPos));        
    }

    // followeeに従う場合は行き先指定なし
    public void Walk(bool flag=true) {
        //State = States.Walk;
        SetState(States.Walk);

        //var anim = GetComponent<Animator>();
        //anim.SetBool("walk", flag);  
        State = flag ? States.Walk : States.Idle;      
    }

    // リーダー（先頭）が乗船
    public IEnumerator GetOn(GameObject vehicle) {
        JustGetOn(vehicle);
        bool isAllOn = false; // 全員乗り込んだかどうか
        var veh = vehicle.GetComponent<VehicleController>();

        // 全員が乗り込むまで船出しない
        while (!isAllOn) {
            //Debug.Log("搭乗中...");
            yield return new WaitForSeconds(0.3f);
            isAllOn = veh.IsReadyToGo();    
        }
    }

    // それ以外が乗船
    public void JustGetOn(GameObject v) {
        State = States.Vehicle;
        Vehicle = v;
        Vehicle.GetComponent<VehicleController>().GetOn(this);
        transform.Find("Label").GetComponent<Renderer>().enabled = false;
        transform.Find("Man").GetComponent<Renderer>().enabled = false;

    }

    public void GetOff() {
        if (!Vehicle) {
            Debug.Log(Name + ":船に乗ってないのに降りられないよ！というエラーが発生");
            return;
        } else {
            Vehicle.GetComponent<VehicleController>().GetOff(this);
        }
        State = followee ? followee.GetComponent<CharaController>().State : States.Idle;
        Vehicle = null;
        transform.Find("Label").GetComponent<Renderer>().enabled = true;
        transform.Find("Man").GetComponent<Renderer>().enabled = true;
    }




    public void SetFollowee(GameObject who) {
        // 移動パスを削除しておく
        GetComponent<CinemachineSplineCart>().Spline = null;

        // 別の人をフォローしている場合は先に解除
        if (followee)
        {
            followee.GetComponent<CharaController>().RemoveFollower(this.gameObject);
        }
        followee = null;

        if (who) {
            followee = who;
            if (transform.position.x == 0) {
                transform.position = who.transform.position;
            }
            LookAtH(transform, who.transform.position);  
            followee.GetComponent<CharaController>().AddFollower(this.gameObject);
        } 
        SetState(followee ?  followee.GetComponent<CharaController>().State : States.Idle);
    }

    public void LookAtH(Transform t, Vector3 p) {
        p.y = t.position.y;
        t.LookAt(p);
    }

    public float GetTerrainHeight(Vector3 pos, bool shipFlag) {
        //return shipFlag ? sea.SampleHeight(pos) : land.SampleHeight(pos);
        return Mathf.Max(
            land.SampleHeight(pos), 
            sea.SampleHeight(pos)
        );
    }

    private Track? GetOldestTrack() {
        return tracks.OldestTrack;
    }

}

class Tracks {
    private List<Track> tracks;
    private int limit;
    #nullable enable
    public Track? OldestTrack { 
        get {return tracks.Count < limit ? null : tracks[0];}
    }

    public Tracks(int delayFrame) {
        tracks = new List<Track>();
        limit = delayFrame;
    }

    public void AddTrack(Transform t) {
        tracks.Add(new Track(t));
        if (tracks.Count > limit) {
            tracks.RemoveAt(0);
        }
    }

}

class Track {
    public Vector3 Position {get;}
    public Vector3 LocalEulerAngles {get;}
    public Track(Transform t) {
        Position = t.position;
        LocalEulerAngles = t.localEulerAngles;
    }
    public string ToString() {
        return Position.x + "," + Position.y  + "," + Position.z;
    }
}



