using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SideViewGameFlowUI : MonoBehaviour
{
    private static readonly string[] Maps = {
        "Map01_RuinedEntrance_SideView", "Map02_Grassland_SideView",
        "Map03_Barbarian_SideView", "Map04_FrozenCrystal_SideView"
    };
    private Health playerHealth;
    private PlayerAttack playerAttack;
    private GameObject resultPanel;
    private Text resultTitle;
    private Button secondaryButton;
    private Text secondaryLabel;
    private float completionCheckAt;
    private bool finished;
    private RectTransform playerHealthFill;
    private Text playerHealthText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (System.Array.IndexOf(Maps, scene.name) < 0) return;
        if (FindAnyObjectByType<SideViewGameFlowUI>() == null)
            new GameObject("SideViewGameFlow").AddComponent<SideViewGameFlowUI>();
    }

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        playerHealth = player.GetComponent<Health>();
        if (playerHealth == null) playerHealth = player.AddComponent<Health>();
        playerAttack = player.GetComponent<PlayerAttack>();
        if (playerAttack == null) playerAttack = player.AddComponent<PlayerAttack>();
        playerHealth.Died += OnPlayerDied;
        playerHealth.Changed += RefreshPlayerHealth;
        BuildUI();
        RefreshPlayerHealth(playerHealth);
        completionCheckAt = Time.unscaledTime + 1f;
    }

    private void Update()
    {
        if (finished || Time.unscaledTime < completionCheckAt) return;
        completionCheckAt = Time.unscaledTime + .35f;
        Health[] health = FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bool foundEnemy = false;
        foreach (Health value in health)
            if (value != playerHealth && value.GetComponent<EnemyAttackTarget>() != null && !value.IsDead)
                { foundEnemy = true; break; }
        if (!foundEnemy) ShowComplete();
    }

    private void BuildUI()
    {
        var canvasObject = new GameObject("SideViewGameplayCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);

        BuildPlayerHealthBar(canvas.transform);

        Button attack = Button(canvas.transform, "AttackButton", "TẤN CÔNG", new Color(.72f,.12f,.08f,.96f));
        RectTransform ar = attack.GetComponent<RectTransform>(); ar.anchorMin = ar.anchorMax = new Vector2(1,0); ar.pivot = new Vector2(1,0);
        ar.anchoredPosition = new Vector2(-42,42); ar.sizeDelta = new Vector2(170,90);
        attack.onClick.AddListener(() => { if (!finished && playerAttack != null) playerAttack.Attack(); });

        resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        resultPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect=resultPanel.GetComponent<RectTransform>(); panelRect.anchorMin=Vector2.zero; panelRect.anchorMax=Vector2.one; panelRect.offsetMin=panelRect.offsetMax=Vector2.zero;
        resultPanel.GetComponent<Image>().color=new Color(.015f,.025f,.06f,.88f);
        resultTitle = Label(resultPanel.transform,"ResultTitle","",42);
        RectTransform titleRect=resultTitle.rectTransform; titleRect.anchorMin=new Vector2(.2f,.58f); titleRect.anchorMax=new Vector2(.8f,.75f); titleRect.offsetMin=titleRect.offsetMax=Vector2.zero;
        Button replay=Button(resultPanel.transform,"ReplayButton","CHƠI LẠI",new Color(.08f,.42f,.58f,1));
        SetCentered(replay.GetComponent<RectTransform>(),new Vector2(-120,-20),new Vector2(210,72));
        replay.onClick.AddListener(Restart);
        secondaryButton=Button(resultPanel.transform,"SecondaryButton","NEXT",new Color(.38f,.18f,.62f,1));
        secondaryLabel=secondaryButton.GetComponentInChildren<Text>();
        SetCentered(secondaryButton.GetComponent<RectTransform>(),new Vector2(120,-20),new Vector2(210,72));
        resultPanel.SetActive(false);
    }

    private void OnPlayerDied(Health _) { if (!finished) ShowResult("BẠN ĐÃ THẤT BẠI", false); }
    private void BuildPlayerHealthBar(Transform parent)
    {
        GameObject frame = new GameObject("PlayerHealthBar", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(parent, false);
        RectTransform frameRect = frame.GetComponent<RectTransform>();
        frameRect.anchorMin = frameRect.anchorMax = new Vector2(0f, 1f);
        frameRect.pivot = new Vector2(0f, 1f);
        frameRect.anchoredPosition = new Vector2(32f, -30f);
        frameRect.sizeDelta = new Vector2(330f, 34f);
        frame.GetComponent<Image>().color = new Color(.025f, .035f, .065f, .96f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(frame.transform, false);
        playerHealthFill = fillObject.GetComponent<RectTransform>();
        playerHealthFill.anchorMin = Vector2.zero;
        playerHealthFill.anchorMax = Vector2.one;
        playerHealthFill.pivot = new Vector2(0f, .5f);
        playerHealthFill.offsetMin = new Vector2(4f, 4f);
        playerHealthFill.offsetMax = new Vector2(-4f, -4f);
        fillObject.GetComponent<Image>().color = new Color(.12f, .82f, .33f, 1f);

        playerHealthText = Label(frame.transform, "HealthText", "", 18);
        RectTransform textRect = playerHealthText.rectTransform;
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
    }

    private void RefreshPlayerHealth(Health value)
    {
        if (value == null || playerHealthFill == null) return;
        float ratio = value.maxHealth <= 0f ? 0f : Mathf.Clamp01(value.currentHealth / value.maxHealth);
        playerHealthFill.anchorMax = new Vector2(ratio, 1f);
        playerHealthFill.offsetMax = new Vector2(ratio >= .001f ? -4f : 0f, -4f);
        if (playerHealthText != null) playerHealthText.text = string.Format("HP  {0:0} / {1:0}", value.currentHealth, value.maxHealth);
    }
    private void ShowComplete() { if (!finished) ShowResult("HOÀN THÀNH MAP", true); }
    private void ShowResult(string title, bool won)
    {
        finished=true; resultTitle.text=title; resultPanel.SetActive(true); Time.timeScale=0;
        secondaryButton.onClick.RemoveAllListeners();
        if (won) { secondaryLabel.text="NEXT"; secondaryButton.onClick.AddListener(NextMap); }
        else { secondaryLabel.text="THOÁT RA"; secondaryButton.onClick.AddListener(MainMenu); }
    }
    private void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
    private void MainMenu(){Time.timeScale=1;SceneManager.LoadScene("MainMenuScene");}
    private void NextMap(){int i=System.Array.IndexOf(Maps,SceneManager.GetActiveScene().name);Time.timeScale=1;SceneManager.LoadScene(i>=0&&i<Maps.Length-1?Maps[i+1]:"MainMenuScene");}
    private void OnDestroy(){if(playerHealth!=null){playerHealth.Died-=OnPlayerDied;playerHealth.Changed-=RefreshPlayerHealth;}Time.timeScale=1;}

    private static Button Button(Transform parent,string name,string text,Color color)
    {
        GameObject go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);go.GetComponent<Image>().color=color;
        Text label=Label(go.transform,"Label",text,24);RectTransform r=label.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return go.GetComponent<Button>();
    }
    private static Text Label(Transform parent,string name,string value,int size){GameObject go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);Text t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;return t;}
    private static void SetCentered(RectTransform r,Vector2 position,Vector2 size){r.anchorMin=r.anchorMax=new Vector2(.5f,.45f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;}
}
