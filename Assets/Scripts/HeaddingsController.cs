using UnityEngine;
using TMPro;

public class HeaddingsController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bookTitle;
    [SerializeField] private TextMeshProUGUI episodeTitle;
    [SerializeField] private GameObject labelsAbraham;
    [SerializeField] private GameObject labelsExodus;
    [SerializeField] private GameObject labelsJoshua;
    [SerializeField] private GameObject labelsGospels;
    [SerializeField] private GameObject labelsActs;
    [SerializeField] private UIController uiController;

    private Transform selectedBook = null;
    private Transform selectedEpisode = null;
    private float orgY;

    private float t = 0;


    private float top = -56; // アニメーションの開始位置
    private float y = -80; // BookButtonの初期位置
    private float margin = 12; // ボタン間の隙間

    private bool showHeaddings = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        showBookButtons(false);
        //orgY = transform.position.y;
        
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    void OnEnable() {
        // transform.position = new Vector3(transform.position.x, 10, transform.position.z);
        // t = 0.0f;
    }

    void OnDisable() {

    }

    public void showBookButtons(bool showFlag, bool animationFlag=true) {
        showHeaddings = showFlag;
        
        // 子オブジェクト（BookButtonオブジェクト）を取得
        y = -160;

        foreach (Transform child in this.transform) {
            //children[count] = child; // 順番に子オブジェクトを取得
            //Debug.Log($"検索方法２： {count} 番目 の子供は {children[count].name} です");
            if (child.tag == "Book") {
                if (showFlag) {
                    child.gameObject.SetActive(true);

                    // 位置を調整
                    if (animationFlag) {
                        child.gameObject.GetComponent<Headding>().SetPositions(top, y);                    
                    } else {
                        child.localPosition = new Vector3(child.localPosition.x, y, child.localPosition.z);
                    }

                    float h = child.gameObject.GetComponent<RectTransform>().sizeDelta.y;
                    y -= (h + margin);
                    
                    ShowEpisodeButtons(child, selectedBook == child); // 各bookに属するエピソードの表示制御

                } else {
                    child.gameObject.SetActive(false);
                }

            }
        }
    }

    public void ShowEpisodeButtons(Transform parent, bool show) {
        float lY = -94;
        float orgY = lY - 80;
        foreach (Transform child in parent) {
            if (child.tag == "Episode") {
                child.gameObject.SetActive(show);
                if (show) {
                    child.GetComponent<Headding>().SetPositions(orgY, lY);
                    float h = child.gameObject.GetComponent<RectTransform>().sizeDelta.y;
                    //child.localPosition = new Vector3(child.localPosition.x, ly, child.localPosition.z);
                    lY -= (h + margin);
                    y -= (h + margin); 
                }
            }
        }

    }

    public void OnClickBibleButton() {
        showBookButtons(!showHeaddings);
    }

    public void SelectBook(Transform bookButton) {
        if (selectedBook == bookButton)
        {
            selectedBook = null;
            selectedEpisode = null;
            bookTitle.text = "";
            episodeTitle.text = "";
        }
        else
        {
            labelsAbraham.SetActive(false);
            labelsExodus.SetActive(false);
            labelsJoshua.SetActive(false);
            labelsGospels.SetActive(false);
            labelsActs.SetActive(false);

            selectedBook = bookButton;
            bookTitle.text = bookButton.GetComponentInChildren<TextMeshProUGUI>().text; // 選択言語による
            selectedEpisode = null;
            episodeTitle.text = "";

            switch (selectedBook.name)
            {
                case "Genesis":
                    labelsAbraham.SetActive(true);
                    break;
                case "Exodus":
                    labelsExodus.SetActive(true);
                    break;
                case "Joshua":
                    labelsJoshua.SetActive(true);
                    break;
                case "Gospels":
                    labelsGospels.SetActive(true);
                    break;
                case "Acts":
                    labelsActs.SetActive(true);
                    break;
            }

            // マップを切り替え
            uiController.SelectMap(bookButton.name);
            // エピソードプレイモードになっている場合は戻す
            uiController.ResetEpisode();

        }
        showBookButtons(true, false); // エピソード開閉時はアニメーションしない
    }

    public void SelectEpisode(Transform episodeButton) {
        if (selectedEpisode == episodeButton)
        {
            selectedEpisode = null;
            episodeTitle.text = "";
            uiController.ResetEpisode();
                      
        }
        else
        {
            selectedEpisode = episodeButton;
            episodeTitle.text = episodeButton.GetComponentInChildren<TextMeshProUGUI>().text;
            showBookButtons(false); // エピソードを選択したら見出しリストは閉じる

            // エピソードの開始
            StartCoroutine(
                uiController.PlayEpisode(episodeButton.name)
            );
            //GameObject.Find("ScenarioController").GetComponent<Scenario>().Play(episodeButton.name);
        }
    }
}
