using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Splines;


//　出エジプト（40年の荒野の旅）
public class Exodus : EpisodeBase
{
    private GameObject moses;
    private GameObject aaron;
    private GameObject miriam;

    private Mobs Israelites;
    private GameObject joshua;
    private GameObject caleb;
    private GameObject eleazar;

    private List<GameObject> scouts; // 偵察隊

    public IEnumerator Play() 
    {
        SetMapAndBook(lv6Map, "Exodus");
        var rameses = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("housesRameses");

        // モーセ
        moses = Spawn(rameses.position);
        var charMoses = Charactorize(moses, "MOSES");

        // アロン
        aaron = Spawn(Vector3.zero);
        var charAaron = Charactorize(aaron, "AARON");
        charAaron.SetFollowee(moses);

        // ミリアム
        miriam = Spawn(Vector3.zero);
        var charMiriam = Charactorize(miriam, "MIRIAM");
        charMiriam.SetFollowee(aaron);

        // 数百万のイスラエルの民
        Israelites = SpawnMobs(100, rameses.position, "MILLIONS_OF_ISRAELITES");
        Israelites.Head.GetComponent<CharaController>().SetFollowee(miriam);

        // カメラはリーダーを単純に真南上方から見下ろす
        SetFollowTarget(moses.transform);//paul.transform;


        // 往路
        yield return AsyncPlay(charMoses.Wander(15.0f)); // Rameses

        var cart = moses.GetComponent<CinemachineSplineCart>();
        var path = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("Path_Exodus").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;    
        
        yield return AsyncPlay(charMoses.Walk(64.5f)); // Sukkoth
        yield return AsyncPlay(charMoses.Wander(2.0f)); 

        yield return AsyncPlay(charMoses.Walk(135.0f)); // Etham
        yield return AsyncPlay(charMoses.Wander(2.0f)); 
        yield return AsyncPlay(charMoses.Walk(222.0f)); // Pi Hahirote 紅海を渡る
        yield return AsyncPlay(charMoses.Wander(5.0f)); 
        AsyncPlay(SplitSea());
        yield return AsyncPlay(charMoses.Wander(3.0f)); 

        yield return AsyncPlay(charMoses.Walk(289.0f)); // Marah
        yield return AsyncPlay(charMoses.Wander(2.0f));
        yield return AsyncPlay(charMoses.Walk(333.6f)); // Elim
        yield return AsyncPlay(charMoses.Wander(2.0f));     
        yield return AsyncPlay(charMoses.Walk(511.6f)); // Rephidim
        yield return AsyncPlay(charMoses.Wander(2.0f)); 

        yield return AsyncPlay(charMoses.Walk(556.6f)); // Wilderness of Sinai モーセとヨシュアシナイ山へ
        yield return AsyncPlay(charMoses.Wander(3.0f));


        charAaron.SetFollowee(null); // モーセ一行から離れる
        // ヨシュアがモーセにつく
        joshua = Spawn(Vector3.zero);
        var charJoshua = Charactorize(joshua, "JOSHUA");
        charJoshua.SetFollowee(moses);
        var path2 = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("Path_Sinai").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path2, 0.0f);
        yield return AsyncPlay(charMoses.Walk(0.0f, 81.4f)); // シナイ山頂
        charMoses.SetState(States.Idle, true);
        yield return WaitForAWhile(3.0f);
        yield return AsyncPlay(charMoses.Walk(159.2f)); // 戻る
        yield return AsyncPlay(charMoses.Wander(2.0f));
        yield return AsyncPlay(charMoses.Walk(0.0f, 81.4f)); // 再びシナイ山頂へ
        charMoses.SetState(States.Idle, true);
        yield return WaitForAWhile(2.0f);
        yield return AsyncPlay(charMoses.Walk(159.2f)); // 戻る
        yield return AsyncPlay(charMoses.Wander(3.0f));

        // アロン、ヨシュアの後ろに
        charAaron.SetFollowee(joshua);
        SwitchPath(cart, path, 556.4f);
        yield return AsyncPlay(charMoses.Walk(556.6f, 596.7f)); // シナイの荒野⇒キブロテ・ハ・タアワ
        yield return AsyncPlay(charMoses.Wander(3.0f));        
        yield return AsyncPlay(charMoses.Walk(673.9f)); // ハツェロテ ミリアムの不満
        yield return AsyncPlay(charMoses.Wander(3.0f));
        yield return AsyncPlay(charMoses.Walk(913.7f)); // パランの荒野（カデシュ・バルネア）
        yield return AsyncPlay(charMoses.Wander(5.0f)); 

        //
        // カナン偵察
        //
        charJoshua.SetFollowee(null);
        charAaron.SetFollowee(null);
        // カレブがヨシュアにつく
        caleb = Spawn(Vector3.zero);
        var charCaleb = Charactorize(caleb, "CALEB");
        charCaleb.SetFollowee(joshua);
        // 残り10人の族長
        scouts = new List<GameObject>();
        GameObject f = caleb; // followee
        for (int n = 1; n <= 10; n++) {
            var p = Spawn(Vector3.zero);
            var charP = Charactorize(p, n == 4 ? "SCOUTS" : "");
            charP.SetFollowee(f);
            f = p;
            scouts.Add(p);
        }

        var cartJoshua = joshua.GetComponent<CinemachineSplineCart>();
        var path3 = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("Path_Rehob").gameObject.GetComponent<SplineContainer>();
        cartJoshua.Spline = path3;
        // カメラは偵察隊にスイッチ
        SetFollowTarget(joshua.transform);     
        yield return AsyncPlay(charJoshua.Walk(0.0f, 1366.0f)); // レボ・ハマテまで行って戻る
        yield return AsyncPlay(charJoshua.Wander(15.0f));
        // 族長インスタンスはここで削除
        scouts[0].GetComponent<CharaController>().SetFollowee(null);
        CastsOut(scouts); 
        // カメラをモーセに戻す
        SetFollowTarget(moses.transform);  
        cartJoshua.Spline = null;
        charJoshua.SetFollowee(moses);
        charAaron.SetFollowee(caleb); // モーセ ヨシュア カレブ アロン ミリアム 民
        yield return AsyncPlay(charMoses.Wander(5.0f)); // カナンになんて行きたくない！40年の放浪開始・・・
        
        //
        yield return AsyncPlay(charMoses.Walk(1014.0f)); // ツィンの荒野
        yield return AsyncPlay(charMoses.Wander(3.0f));
        // ミリアム召天
        Israelites.Head.GetComponent<CharaController>().SetFollowee(aaron);
        charMiriam.SetFollowee(null);
        Leave(miriam);
        yield return AsyncPlay(charMoses.Wander(5.0f));

        yield return AsyncPlay(charMoses.Walk(1109.0f)); // エドムとの交渉
        yield return AsyncPlay(charMoses.Wander(2.0f));
        yield return AsyncPlay(charMoses.Walk(1171.0f)); // ホル山近く
        yield return AsyncPlay(charMoses.Wander(4.0f));
        // モーセ（ヨシュア）、アロン、エルアザル、ホル山へ
        charCaleb.SetFollowee(null);
        Israelites.Head.GetComponent<CharaController>().SetFollowee(caleb);
        charAaron.SetFollowee(joshua);
        eleazar = Spawn(Vector3.zero);
        var charEleazar = Charactorize(eleazar, "ELEAZAR");
        charEleazar.SetFollowee(aaron);
        var path4 = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("Path_Hor").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path4, 0.0f);
        yield return AsyncPlay(charMoses.Walk(0.0f, 59.3f)); // ホル山山頂
        charMoses.SetState(States.Idle, true);
        yield return WaitForAWhile(3.0f);
        charAaron.SetFollowee(null);
        charEleazar.SetFollowee(joshua);
        // アロン召天
        charAaron.SetFollowee(null);
        Leave(aaron);
        yield return WaitForAWhile(3.0f);
        yield return AsyncPlay(charMoses.Walk(117.6715f)); // ホル山下山
        // エルアザルはここでMOBの中に戻る
        charEleazar.SetFollowee(null);
        CastOut(eleazar);

        // 荒野をぐるぐる
        charCaleb.SetFollowee(joshua);
        Israelites.Head.GetComponent<CharaController>().SetFollowee(caleb);
        SwitchPath(cart, path, 1171.0f); // ホル山近くまで戻って民と合流
        yield return AsyncPlay(charMoses.Walk(1171.0f, 1498.0f)); // ぐるぐるの終点

        yield return AsyncPlay(charMoses.Walk(1235.0f, 1498.0f)); // ぐるぐるの始点から終点へ一周
        yield return AsyncPlay(charMoses.Walk(1235.0f, 1498.0f)); // ぐるぐるの始点から終点へ一周

        yield return AsyncPlay(charMoses.Walk(2054.0f)); // モアブの草原
        yield return AsyncPlay(charMoses.Wander(103.0f));
 

        // モーセ（とヨシュア）、ネボ山へ
        charCaleb.SetFollowee(null);
        var path5 = lv6Map.transform.Find("PlaceLabels").Find("Exodus").Find("Path_Nebo").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path5, 0.0f);
        yield return AsyncPlay(charMoses.Walk(0.0f, 39.80337f)); // ピスガの頂
        charMoses.SetState(States.Idle, true);        
        yield return WaitForAWhile(5.0f);
        charJoshua.SetFollowee(null);
        cart.Spline = null;
        Leave(moses);// モーセ召天
        yield return WaitForAWhile(5.0f);
        //charMoses.Stroll();  
    }

    public IEnumerator SplitSea() {
        // モーセ海を割る
        var seaN = currentMap.transform.Find("MosesSeaN");
        var seaS = currentMap.transform.Find("MosesSeaS");
        for (var n = 5; n > 0; n--) {
            var pN = seaN.position;
            pN.z += 3;
            seaN.position = pN;
            var pS = seaS.position;
            pS.z -= 3;
            seaS.position = pS;
            yield return WaitForAWhile(0.1f);
        }
        yield return WaitForAWhile(8.0f);
        for (var n = 5; n > 0; n--) {
            var pN = seaN.position;
            pN.z -= 2;
            seaN.position = pN;
            var pS = seaS.position;
            pS.z += 2;
            seaS.position = pS;
            yield return WaitForAWhile(0.1f);
        }
    } 

}
