using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Splines;


//　パウロの第一次伝道旅行
public class ActsJourney1 : EpisodeBase
{
    private GameObject barnabas;
    private GameObject paul;
    private GameObject mark;

    public IEnumerator Play() 
    {
        SetMapAndBook(lv4Map, "Acts");
        var antioch = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesAntioch");

        // バルナバ
        barnabas = Spawn(antioch.position);
        var charBarnabas = Charactorize(barnabas, "BARNABAS");
        charBarnabas.Name = "Barnabas";

        // パウロ
        paul = Spawn(Vector3.zero);
        var charPaul = Charactorize(paul, "PAUL");
        charPaul.SetFollowee(barnabas);
        charPaul.Name = "Paul";

        // マルコ（サラミスで合流）
        var salamis = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesSalamis");
        mark = Spawn(salamis.position);
        var charMark = Charactorize(mark, "MARK");
        charMark.Name = "Mark";

        // カメラはリーダーを単純に真南上方から見下ろす
        SetFollowTarget(barnabas.transform);//paul.transform;

        // 往路
        yield return AsyncPlay(charBarnabas.Wander(15.0f)); // Antioch

        var cart = barnabas.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Asia").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;    
        yield return AsyncPlay(charBarnabas.Walk(13.0f)); // => Seleucia
       
        yield return charBarnabas.GetOn(FindVehicle("Boat_Ant"));
        yield return AsyncPlay(charBarnabas.Move(89.2f)); // => Salamis湾
        charBarnabas.GetOff();
        yield return AsyncPlay(charBarnabas.Walk(93.6f)); // => Salamis       
        charMark.SetFollowee(paul);
        yield return AsyncPlay(charBarnabas.Wander(1));

        yield return AsyncPlay(charBarnabas.Walk(142.0f)); // => Phaphos
        yield return AsyncPlay(charBarnabas.Wander(1));
        // リーダーをバルナバからパウロへ
        charPaul.SetFollowee(null);
        charBarnabas.SetFollowee(paul);
        charMark.SetFollowee(barnabas);
        cart.Spline = null;
        cart = paul.GetComponent<CinemachineSplineCart>();
        cart.Spline = path;
        SetFollowTarget(paul.transform); // カメラの追随キャラを変更

        yield return AsyncPlay(charPaul.Walk(139.9f, 145.2f)); // => Phaphos湾
        yield return charPaul.GetOn(FindVehicle("Boat_Pha"));

        yield return AsyncPlay(charPaul.Move(260.1f)); // Perge湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(274.9f)); // Perge
        yield return AsyncPlay(charPaul.Wander(1));
        // マルコが離脱
        charMark.SetFollowee(null);
        var cart2 = mark.GetComponent<CinemachineSplineCart>();
        var path2 = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Per_Jer").gameObject.GetComponent<SplineContainer>();
        cart2.Spline = path2;   
        AsyncPlay(playSubEpisode());

        yield return AsyncPlay(charPaul.Walk(326.6f)); // => Antioch in Picidia        
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(382.0f)); // => Iconium
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(397.0f)); // => Lystra
        yield return AsyncPlay(charPaul.Wander(2));
        
        yield return AsyncPlay(charPaul.Walk(439.4f)); // => Derbe
        yield return AsyncPlay(charPaul.Wander(2));

        // 復路
        yield return AsyncPlay(charPaul.Walk(601.4f)); // => Perge
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(618.7f)); // Attalia湾
        yield return charPaul.GetOn(FindVehicle("Boat_Att"));

        yield return AsyncPlay(charPaul.Move(809.9f)); // Antioch湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(825.4f)); // => Antioch

        yield return AsyncPlay(charPaul.Wander(3));
        charBarnabas.SetState(States.Stroll, true);
        yield return WaitForAWhile(3.0f);
    }

    // マルコの離脱
    public IEnumerator playSubEpisode() {
        var charMark = mark.GetComponent<CharaController>();
        yield return AsyncPlay(charMark.Walk(0.0f, 15.7f));
        yield return charMark.GetOn(FindVehicle("Boat_Pha"));
        yield return AsyncPlay(charMark.Move(266.7f));
        charMark.GetOff();
        yield return AsyncPlay(charMark.Walk(294.6f));
        charMark.Walk(false); // idle状態
    }

}
