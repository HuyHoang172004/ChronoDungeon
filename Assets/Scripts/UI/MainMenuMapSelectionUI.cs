using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuMapSelectionUI : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.name != "MainMenuScene" || FindAnyObjectByType<MainMenuMapSelectionUI>() != null) return;
        new GameObject("MapSelectionBootstrap").AddComponent<MainMenuMapSelectionUI>();
    }

    private void Awake()
    {
        MainMenu menu = FindAnyObjectByType<MainMenu>();
        if (menu == null) return;
        GameObject oldPlay = GameObject.Find("PlayButton"); if (oldPlay != null) oldPlay.SetActive(false);
        var canvasObject = new GameObject("MapSelectionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=300;
        CanvasScaler scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);
        GameObject panel=new GameObject("MapSelectionPanel",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
        panel.transform.SetParent(canvas.transform,false);RectTransform rect=panel.GetComponent<RectTransform>();
        rect.anchorMin=new Vector2(.3f,.15f);rect.anchorMax=new Vector2(.7f,.82f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        panel.GetComponent<Image>().color=new Color(.02f,.05f,.1f,.94f);
        VerticalLayoutGroup layout=panel.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(35,35,30,30);layout.spacing=14;layout.childControlHeight=true;layout.childControlWidth=true;
        AddLabel(panel.transform,"CHỌN MAP",34,70);
        string[] names={"MAP 1 — RUINED ENTRANCE","MAP 2 — GRASSLAND","MAP 3 — BARBARIAN STRONGHOLD","MAP 4 — FROZEN CRYSTAL"};
        for(int i=0;i<4;i++){int map=i+1;Button b=AddButton(panel.transform,names[i]);b.onClick.AddListener(()=>{menu.SelectMap(map);menu.PlayGame();});}
    }
    private static void AddLabel(Transform parent,string value,int size,float height){GameObject go=new GameObject("Title",typeof(RectTransform),typeof(Text),typeof(LayoutElement));go.transform.SetParent(parent,false);Text t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.5f,.92f,1);go.GetComponent<LayoutElement>().preferredHeight=height;}
    private static Button AddButton(Transform parent,string value){GameObject go=new GameObject(value,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));go.transform.SetParent(parent,false);go.GetComponent<Image>().color=new Color(.08f,.25f,.4f,1);go.GetComponent<LayoutElement>().preferredHeight=72;GameObject label=new GameObject("Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(go.transform,false);RectTransform r=label.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;Text t=label.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=22;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;return go.GetComponent<Button>();}
}
