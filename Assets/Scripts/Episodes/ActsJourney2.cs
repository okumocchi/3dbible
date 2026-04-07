using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Splines;


//　パウロの第2次伝道旅行
public class ActsJourney2 : EpisodeBase
{
    private GameObject paul;
    private GameObject silas;
    private GameObject barnabas;
    private GameObject mark;
    private GameObject timothy;
    private GameObject luke;
    private GameObject aquila;
    private GameObject priscilla;

    public IEnumerator Play() 
    {
        //var currentMap = lv4Map;
        SetMapAndBook(lv4Map, "Acts");

        FindVehicle("Boat_Tro2").SetActive(true);
        FindVehicle("Boat_Bal").SetActive(true);
        FindVehicle("Boat_Bal2").SetActive(true);
        FindVehicle("Boat_Ant").SetActive(true);
        FindVehicle("Boat_Cen").SetActive(true);


        FindVehicle("Boat_Eph").SetActive(false);
        FindVehicle("Boat_Eph2").SetActive(false);

        var antioch = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesAntioch");

        // パウロ
        paul = Spawn(antioch.position);
        var charPaul = Charactorize(paul, "PAUL");

        // シラス
        silas =  Spawn(Vector3.zero);
        var charSilas = Charactorize(silas, "SILAS");
        charSilas.SetFollowee(paul);

        // バルナバ
        barnabas = Spawn(antioch.position);
        var charBarnabas =  Charactorize(barnabas, "BARNABAS");

        // マルコ
        mark = Spawn(Vector3.zero);
        var charMark = Charactorize(mark, "MARK");
        charMark.SetFollowee(barnabas);

        // テモテ（リステラで合流）
        var lystra = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesLystra");
        timothy = Spawn(lystra.position);
        var charTimothy =  Charactorize(timothy, "TIMOTHY");

        // ルカ（トロアスで合流）
        var troas = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesTroas");
        luke = Spawn(troas.position);
        var charLuke =  Charactorize(luke, "LUKE");

        // アクラとプリスキラ（コリントで合流）
        var corinth = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesCorinth");
        aquila = Spawn(corinth.position);
        var charAquila = Charactorize(aquila, "AQUILA");
        priscilla = Spawn(Vector3.zero);
        var charPriscilla = Charactorize(priscilla, "PRISCILLA");

        // 追随カメラ
        SetFollowTarget(paul.transform);

        // Antioch 
        AsyncPlay(playSubEpisode()); // ここでバルナバとマルコが別行動

        yield return AsyncPlay(charPaul.Wander(15.0f));
        var cart = paul.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path2").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;

        yield return AsyncPlay(charPaul.Walk(215.8f)); // => Lystra
        charTimothy.SetFollowee(silas); // テモテ合流
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(509.1f)); // = Troas
        charLuke.SetFollowee(timothy); // ルカ合流
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(517.2f)); // Troas湾
        yield return AsyncPlay(charPaul.GetOn(FindVehicle("Boat_Tro2")));

        yield return AsyncPlay(charPaul.Move(591.0f)); // => Neapolis湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(602.5f)); // => Philippi
        yield return AsyncPlay(charPaul.Wander(2));
        charLuke.SetFollowee(null); // ここでルカは離脱

        yield return AsyncPlay(charPaul.Walk(652.5f)); // =>Thesaronica
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(687.3f)); // =>Bellair
        yield return AsyncPlay(charPaul.Wander(1));
        charSilas.SetFollowee(null); // シラスとテモテ、パウロと別行動
        AsyncPlay(playSubEpisode2()); // しばらく待ってからコリントへ

        yield return AsyncPlay(charPaul.Walk(713.0f)); // Bellair湾 
        yield return AsyncPlay(charPaul.GetOn(FindVehicle("Boat_Bal")));

        yield return AsyncPlay(charPaul.Move(902.1f)); // Athenai湾 
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(909.8f)); // Athenai
        yield return AsyncPlay(charPaul.Wander(2));

        yield return AsyncPlay(charPaul.Walk(950.7f)); // =>Corinth
        charAquila.SetFollowee(paul); // アクラとプリスキラ合流
        charPriscilla.SetFollowee(aquila); // 
        yield return AsyncPlay(charPaul.Wander(13)); // この間にシラスとテモテが追いついて合流

        yield return AsyncPlay(charPaul.Walk(967.5f)); // => Cencrea湾
        var boatCen = FindVehicle("Boat_Cen");
        yield return AsyncPlay(charPaul.GetOn(boatCen));

        yield return AsyncPlay(charPaul.Move(1114.0f)); // Ephesus湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(1121.0f)); // =>Ephesus
        yield return AsyncPlay(charPaul.Wander(3)); // 
        charAquila.SetFollowee(null); // アクラとプリスキラはエペソに残留
        charAquila.SetState(States.Stroll);
        yield return AsyncPlay(charPaul.Walk(1129.0f)); // Ephesus湾
        yield return AsyncPlay(charPaul.GetOn(boatCen));

        yield return AsyncPlay(charPaul.Move(1516)); // => Caesarea湾
        charPaul.GetOff();

        yield return AsyncPlay(charPaul.Walk(1797.594f)); // => Antioch
        yield return AsyncPlay(charPaul.Wander(5)); 
        charPaul.SetState(States.Stroll, true);
        yield return WaitForAWhile(3.0f);
    }

    // バルナバとマルコ、すったもんだでキプロスへ
    public IEnumerator playSubEpisode() {
        var charBarnabas = barnabas.GetComponent<CharaController>();
        var charMark = mark.GetComponent<CharaController>();
        var cart = barnabas.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Ant_Cyp").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;
        cart.SplinePosition = 692.5f; // Bellair
        yield return AsyncPlay(charBarnabas.Wander(3));
        yield return AsyncPlay(charBarnabas.Walk(0.0f, 14.0f));

        yield return charBarnabas.GetOn(FindVehicle("Boat_Ant"));
        yield return AsyncPlay(charBarnabas.Move(88.6f));
        charBarnabas.GetOff();
        yield return AsyncPlay(charBarnabas.Walk(104.3f));
        yield return AsyncPlay(charBarnabas.WalkCircle(charBarnabas.gameObject.transform.position, 10.0f, 60.0f));
        charMark.SetState(States.Idle); // idle状態

    }

    // シラスとテモテ、パウロに遅れてコリントへ
    public IEnumerator playSubEpisode2() {
        var charSilas = silas.GetComponent<CharaController>();
        yield return WaitForAWhile(5.0f); 

        var cart = silas.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path2").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;
        yield return AsyncPlay(charSilas.Walk(687.0f, 709.4f)); // Bellair湾
        yield return charSilas.GetOn(FindVehicle("Boat_Bal2"));
        yield return AsyncPlay(charSilas.Move(898.1f)); // Athenai湾
        charSilas.GetOff();
        yield return AsyncPlay(charSilas.Walk(950.3f)); // Corinth
        cart.Spline = null;
        charSilas.SetFollowee(paul);
    }

}
