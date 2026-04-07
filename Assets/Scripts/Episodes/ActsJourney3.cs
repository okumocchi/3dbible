using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Splines;




//　パウロの第3次伝道旅行
public class ActsJourney3 : EpisodeBase
{
    private GameObject paul;
    private GameObject apollos;
    private GameObject luke;
    private GameObject sopater;
    private GameObject erastus;
    private GameObject aristarchus;
    private GameObject secundus;
    private GameObject gaius;
    private GameObject timothy;
    private GameObject tychicus;
    private GameObject trophimus;
    private GameObject titus;
    
    public IEnumerator Play() 
    {
        SetMapAndBook(lv4Map, "Acts");
        
        FindVehicle("Boat_Eph").SetActive(true);
        FindVehicle("Boat_Bal").SetActive(true);
        FindVehicle("Boat_Ass").SetActive(true);
        FindVehicle("Boat_Pat").SetActive(true);
        FindVehicle("Boat_Eph2").SetActive(true);

        FindVehicle("Boat_Tro").SetActive(false);
        FindVehicle("Boat_Tro2").SetActive(false);
        FindVehicle("Boat_Bal2").SetActive(false);
        FindVehicle("Boat_Cen").SetActive(false);
        FindVehicle("Boat_Nea").SetActive(false);

     // アンテオケから出発！
        var antioch = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesAntioch");

        // パウロ
        paul = Spawn(antioch.position);
        var charPaul = Charactorize(paul, "PAUL");
        // カメラはパウロを追跡
        SetFollowTarget(paul.transform);     

        // ソパテロ、エラスト、アリスタルコ、セクンド、ガイオ、テモテ、ティキコ、トロフィモは最初から同行？
        sopater = Spawn(antioch.position);
        var charSopater = Charactorize(sopater, "SOPATER");
        charSopater.SetFollowee(paul);
        erastus = Spawn(antioch.position);
        var charErastus = Charactorize(erastus, "ERASTUS");
        charErastus.SetFollowee(sopater);
        aristarchus = Spawn(antioch.position);
        var charAristarchus = Charactorize(aristarchus, "ARISTARCHUS");
        charAristarchus.SetFollowee(erastus);
        secundus = Spawn(antioch.position);
        var charSecundus = Charactorize(secundus, "SECUNDUS");
        charSecundus.SetFollowee(aristarchus);
        gaius = Spawn(antioch.position);
        var charGaius = Charactorize(gaius, "GAIUS");
        charGaius.SetFollowee(secundus);
        timothy = Spawn(antioch.position);
        var charTimothy = Charactorize(timothy, "TIMOTHY");
        charTimothy.SetFollowee(gaius);
        tychicus = Spawn(antioch.position);
        var charTychicus = Charactorize(tychicus, "TYCHICUS");
        charTychicus.SetFollowee(timothy);
        trophimus = Spawn(antioch.position);
        var charTrophimus = Charactorize(trophimus, "TROPHIMUS");
        charTrophimus.SetFollowee(tychicus);

        // アポロはコリントにいる
        var corinth = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesCorinth");
        apollos = Spawn(corinth.position);
        var charApollos = Charactorize(apollos, "APOLLOS");
        charApollos.SetState(States.Stroll);

        // ルカも最初から同行していた可能性あり=>ピリピ教会の代表だったぽい
        var philippi = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("housesPhilippi");
        luke = Spawn(philippi.position);
        var charLuke = Charactorize(luke, "LUKE");
        charLuke.SetState(States.Stroll);
        // テトスもピリピにいた
        titus = Spawn(philippi.position);
        var charTitus = Charactorize(titus, "TITUS");
        charTitus.SetState(States.Stroll);
   
        // スタート！
        yield return AsyncPlay(charPaul.Wander(15.0f));
        var cart = paul.GetComponent<CinemachineSplineCart>();
        var path2 = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path2").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path2;

        yield return AsyncPlay(charPaul.Walk(287.3f)); // => Antiok(Picidea)
        // エペソルートへ乗り換え
        var path3 = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Ant_Eph_Tro").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path3, 0.0f);
        yield return AsyncPlay(charPaul.Walk(0.0f, 173.6f)); // => Ephesus
        yield return AsyncPlay(charPaul.Wander(12.0f));

        // テモテとエラストを先にマケドニア（多分ピリピ）に遣わす
        charTimothy.SetFollowee(null);
        charErastus.SetFollowee(timothy);
        charAristarchus.SetFollowee(sopater);
        charTychicus.SetFollowee(gaius);
        AsyncPlay(PlaySubEpisode());

        yield return AsyncPlay(charPaul.Wander(15.0f)); // エペソでの騒ぎ

        yield return AsyncPlay(charPaul.Walk(179.2f)); 
        var boatEph = FindVehicle("Boat_Eph");
        yield return charPaul.GetOn(boatEph);
        yield return AsyncPlay(charPaul.Move(305.9f)); // => Troas湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(314.5f)); // Troas
        yield return AsyncPlay(charPaul.Wander(15.0f));

        SwitchPath(cart, path2, 509.5f);
        yield return AsyncPlay(charPaul.Walk(509.5f, 514.0f)); 
        yield return charPaul.GetOn(boatEph);

        yield return AsyncPlay(charPaul.Move(588.0f)); // Phillipi湾（先についている船の手前につける）
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(603.2f)); // Phillipi
        yield return AsyncPlay(charPaul.Wander(10.0f));
        charLuke.SetFollowee(paul); // ルカと合流
        charSopater.SetFollowee(luke);
        charTimothy.SetFollowee(trophimus); //テモテ・エラストも合流
        yield return AsyncPlay(charPaul.Wander(15.0f));

        yield return AsyncPlay(charPaul.Walk(712.6f)); // Ballaire湾
        var boatBal = FindVehicle("Boat_Bal");
        yield return charPaul.GetOn(boatBal);

        yield return AsyncPlay(charPaul.Move(871.6f)); // => アテネの手前の海路

        // 直接コリントへのルートに乗り換え
        var path4 = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Aka_Jer").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path4, 0.0f);
        yield return AsyncPlay(charPaul.Move(0.0f, 39.4f)); // => コリント湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(49.8f)); // => コリント
        yield return AsyncPlay(charPaul.Wander(15.0f));
        charSopater.SetFollowee(null);  // ルカを除いた弟子たちが別行動
        AsyncPlay(PlaySubEpisode2()); // 弟子たちは海路でトロアスへ
        
        // パウロとルカは陸路でピリピへ        
        yield return AsyncPlay(charPaul.Walk(311.8f)); // => Neapolis湾
        yield return charPaul.GetOn(FindVehicle("Boat_Eph"));
        yield return AsyncPlay(charPaul.Move(388.9f)); // => Troas湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(393.1f)); // => Troas
        charSopater.SetFollowee(luke); // 先についていた弟子たちと合流
        yield return AsyncPlay(charPaul.Wander(5.0f)); // ユテコ事件

        charLuke.SetFollowee(null); // パウロは一人で陸路Assosへ
        AsyncPlay(PlaySubEpisode3()); // 他は海路でAssosへ。こっちの方が遠い
        yield return AsyncPlay(charPaul.Wander(5.0f)); // 
        yield return AsyncPlay(charPaul.Walk(404.3f)); // => Assos
        yield return AsyncPlay(charPaul.Wander(20.0f)); // 海路組の到着を待つ
        charLuke.SetFollowee(paul); // 海路組、パウロと合流
        yield return AsyncPlay(charPaul.Wander(7.0f)); // 
        yield return AsyncPlay(charPaul.Walk(411.5f)); // => Assos湾
        var boatAss = FindVehicle("Boat_Ass");
        yield return charPaul.GetOn(boatAss);
        yield return AsyncPlay(charPaul.Move(424.2f)); // => Mitylene湾
        charPaul.GetOff(); 
        yield return AsyncPlay(charPaul.Walk(427.5f)); //  Mytilene      
        yield return AsyncPlay(charPaul.Wander(15.0f));
        yield return AsyncPlay(charPaul.Walk(430.0f)); //  Mytilene湾     
        yield return charPaul.GetOn(boatAss);

        yield return AsyncPlay(charPaul.Move(525.0f)); // => Miletus湾
        charPaul.GetOff();
        yield return AsyncPlay(charPaul.Walk(529.1f)); //  Miletus      
        yield return AsyncPlay(charPaul.Wander(15.0f)); // 涙の別離
        yield return AsyncPlay(charPaul.Walk(532.6f)); //  Mytilene湾     
        yield return charPaul.GetOn(boatAss);

        yield return AsyncPlay(charPaul.Move(650.5f)); // => Patara湾
        charPaul.GetOff(); 
        yield return AsyncPlay(charPaul.Walk(660.2f)); // Patara      
        yield return AsyncPlay(charPaul.Wander(15.0f)); 
        yield return AsyncPlay(charPaul.Walk(669.8f)); //  Patara湾     
        var boatPat = FindVehicle("Boat_Pat");
        yield return charPaul.GetOn(boatPat);

        yield return AsyncPlay(charPaul.Move(918.0f)); // => Tyre湾
        charPaul.GetOff(); 
        yield return AsyncPlay(charPaul.Walk(924.3f)); // Tyre      
        yield return AsyncPlay(charPaul.Wander(15.0f)); 
        yield return AsyncPlay(charPaul.Walk(929.7f)); //  Tyre湾     
        yield return charPaul.GetOn(boatPat);

        yield return AsyncPlay(charPaul.Move(941.8f)); // => Ptlemais湾
        charPaul.GetOff(); 
        yield return AsyncPlay(charPaul.Walk(946.3f)); // Ptlemais      
        yield return AsyncPlay(charPaul.Wander(15.0f)); 
        yield return AsyncPlay(charPaul.Walk(952.3f)); // Ptlemais湾
        yield return charPaul.GetOn(boatPat);

        yield return AsyncPlay(charPaul.Move(970.0f)); // => Caesarea湾
        charPaul.GetOff(); 
        yield return AsyncPlay(charPaul.Walk(975.1f)); // => Caesarea
        yield return AsyncPlay(charPaul.Wander(15.0f)); 
        yield return AsyncPlay(charPaul.Walk(1012.9f)); // => エルサレム
        yield return AsyncPlay(charPaul.Wander(20.0f)); 
        charPaul.SetState(States.Stroll, true);
        yield return WaitForAWhile(3.0f);

    }

    // テモテとエラスト、先にピリピへ
    public IEnumerator PlaySubEpisode() {
        var charTimothy = timothy.GetComponent<CharaController>();
        var cart = timothy.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Ant_Eph_Tro").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;
        yield return AsyncPlay(charTimothy.Walk(173.6f, 182.0f)); 
        var boatEph2 = FindVehicle("Boat_Eph2");  
        yield return charTimothy.GetOn(boatEph2);
        yield return AsyncPlay(charTimothy.Move(303.6f)); // => Troas湾
        charTimothy.GetOff();
        yield return AsyncPlay(charTimothy.Walk(308.9f)); // Troas

        path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path2").gameObject.GetComponent<SplineContainer>();
        SwitchPath(cart, path, 509.5f);
        yield return AsyncPlay(charTimothy.Walk(509.5f, 516.8f)); 
        //var boat2 = FindVehicle("Boat_Tro2");        
        yield return charTimothy.GetOn(boatEph2);

        yield return AsyncPlay(charTimothy.Move(593.1f)); // Phillip湾 
        charTimothy.GetOff();
        yield return AsyncPlay(charTimothy.Walk(603.2f)); // Phillip
        
        cart.Spline = null;        
        charTimothy.SetState(States.Stroll);
    }

    // 弟子たち海路で先にトロアスへ
    public IEnumerator PlaySubEpisode2() {
        var charSopater = sopater.GetComponent<CharaController>();
        var cart = sopater.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Cor_Ass").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;
        yield return AsyncPlay(charSopater.Walk(0.0f, 11.2f)); 

        yield return charSopater.GetOn(FindVehicle("Boat_Bal"));//  Ballaier発の船がコリントに停泊中  
        yield return AsyncPlay(charSopater.Move(174.8f)); // => Troas
        charSopater.GetOff();
        yield return AsyncPlay(charSopater.Walk(178.8f)); 
        cart.Spline = null;
        charSopater.SetState(States.Stroll, true);

    }

    // パウロ以外は海路でアソスへ
    public IEnumerator PlaySubEpisode3() {
        var charLuke = luke.GetComponent<CharaController>();
        var cart = luke.GetComponent<CinemachineSplineCart>();
        var path = lv4Map.transform.Find("PlaceLabels").Find("Acts").Find("Path_Cor_Ass").gameObject.GetComponent<SplineContainer>();
        cart.Spline = path;
        yield return AsyncPlay(charLuke.Walk(178.8f, 184.1f)); // Troas湾
        yield return charLuke.GetOn(FindVehicle("Boat_Bal"));
        yield return AsyncPlay(charLuke.Move(214.1f)); // => Assos湾
        charLuke.GetOff();
        yield return AsyncPlay(charLuke.Walk(219.9f)); // Assos
        cart.Spline = null;
        charLuke.SetState(States.Stroll);
    }

}
