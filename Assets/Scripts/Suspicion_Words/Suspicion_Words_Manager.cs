using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Suspicion_Words_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("References -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;
    [SerializeField] private Prompt_List_SO Prompt_List_SO;
    [Space]
    [Header("Word Listes ------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private List<TextMeshProUGUI> Motives = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> Locations = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> Weapons = new List<TextMeshProUGUI>();
    [Space]
    [Header("Button References ------------------------------------------------------------------")]
    [Space]
    [SerializeField] private List<Button> Motive_Buttons = new List<Button>();
    [SerializeField] private List<Button> Location_Buttons = new List<Button>();
    [SerializeField] private List<Button> Weapon_Buttons = new List<Button>();
    [SerializeField] private Button Prediction_Button;
    [Space]
    [Header("Button Colors ----------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Color Normal_Color = Color.white;
    [SerializeField] private Color Selected_Color = Color.green;
    [Space]
    [Header("Typewriter Settings ----------------------------------------------------------------")]
    [Space]
    [SerializeField] private float Typing_Speed = 0.05f;
    [SerializeField] private float Delay_Between_Words = 0.2f;

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Variables
    
    private List<Coroutine> Active_Coroutines = new List<Coroutine>();
    
    // Seçili buton indexleri (-1 = hiçbiri seçili değil)
    private int Selected_Motive_Index = -1;
    private int Selected_Location_Index = -1;
    private int Selected_Weapon_Index = -1;
    
    // Oyuncunun tahmini
    public string Player_Prediction { get; private set; } = "";

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Unity Lifecycle

    private void Start()
    {
        Setup_Button_Listeners();
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Public Methods

    public void Update_Suspicion_Words_UI()
    {
        Stop_All_Typing();
        StartCoroutine(Type_All_Words());
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Methods - Setup

    private void Setup_Button_Listeners()
    {
        // Motive butonları
        for (int i = 0; i < Motive_Buttons.Count; i++)
        {
            int index = i; // Closure için local copy
            Motive_Buttons[i].onClick.AddListener(() => On_Motive_Button_Clicked(index));
        }

        // Location butonları
        for (int i = 0; i < Location_Buttons.Count; i++)
        {
            int index = i;
            Location_Buttons[i].onClick.AddListener(() => On_Location_Button_Clicked(index));
        }

        // Weapon butonları
        for (int i = 0; i < Weapon_Buttons.Count; i++)
        {
            int index = i;
            Weapon_Buttons[i].onClick.AddListener(() => On_Weapon_Button_Clicked(index));
        }

        // Prediction butonu
        if (Prediction_Button != null)
        {
            Prediction_Button.onClick.AddListener(On_Prediction_Button_Clicked);
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Methods - Button Callbacks

    private void On_Motive_Button_Clicked(int button_Index)
    {
        // Önceki seçimi temizle
        if (Selected_Motive_Index >= 0 && Selected_Motive_Index < Motive_Buttons.Count)
        {
            Set_Button_Color(Motive_Buttons[Selected_Motive_Index], Normal_Color);
        }

        // Yeni seçimi kaydet ve renklendir
        Selected_Motive_Index = button_Index;
        Set_Button_Color(Motive_Buttons[button_Index], Selected_Color);
    }

    private void On_Location_Button_Clicked(int button_Index)
    {
        // Önceki seçimi temizle
        if (Selected_Location_Index >= 0 && Selected_Location_Index < Location_Buttons.Count)
        {
            Set_Button_Color(Location_Buttons[Selected_Location_Index], Normal_Color);
        }

        // Yeni seçimi kaydet ve renklendir
        Selected_Location_Index = button_Index;
        Set_Button_Color(Location_Buttons[button_Index], Selected_Color);
    }

    private void On_Weapon_Button_Clicked(int button_Index)
    {
        // Önceki seçimi temizle
        if (Selected_Weapon_Index >= 0 && Selected_Weapon_Index < Weapon_Buttons.Count)
        {
            Set_Button_Color(Weapon_Buttons[Selected_Weapon_Index], Normal_Color);
        }

        // Yeni seçimi kaydet ve renklendir
        Selected_Weapon_Index = button_Index;
        Set_Button_Color(Weapon_Buttons[button_Index], Selected_Color);
    }

        private void On_Prediction_Button_Clicked()
        {
        // Her kategoriden seçim yapıldı mı kontrol et
        if (!Is_All_Categories_Selected())
        {
            Debug.LogWarning("Tüm kategorilerden seçim yapılmadı!");
            return;
        }

        // Player_Prediction string'ini oluştur
        string selected_Motive = Motives[Selected_Motive_Index].text;
        string selected_Location = Locations[Selected_Location_Index].text;
        string selected_Weapon = Weapons[Selected_Weapon_Index].text;

        Player_Prediction = Prompt_List_SO.Prompt_List[2].Paragraph + "\n Player Prediction:" + selected_Motive + "," + selected_Location + "," + selected_Weapon + "\n Murders Scenario:" + Text_Seperator_0.Murder_Scenario;

        Debug.Log("Player Prediction Created: " + Player_Prediction);
    }

    public bool Is_All_Categories_Selected()
    {
        return Selected_Motive_Index >= 0 && Selected_Location_Index >= 0 && Selected_Weapon_Index >= 0;
    }


    public void Prepare_Player_Prediction()
    {
        On_Prediction_Button_Clicked();
    }
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Methods - Color Management

    private void Set_Button_Color(Button button, Color color)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.selectedColor = color;
            colors.highlightedColor = color;
            button.colors = colors;
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Methods - Typewriter

    private void Stop_All_Typing()
    {
        foreach (var coroutine in Active_Coroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        Active_Coroutines.Clear();
    }

    private IEnumerator Type_All_Words()
    {
        Clear_All_Texts();
        
        for (int i = 0; i < 3; i++)
        {
            var motiveCoroutine = StartCoroutine(Type_Text(Motives[i], Text_Seperator_0.Motives_For_Murder[i]));
            Active_Coroutines.Add(motiveCoroutine);
            
            var locationCoroutine = StartCoroutine(Type_Text(Locations[i], Text_Seperator_0.Murder_Locations[i]));
            Active_Coroutines.Add(locationCoroutine);
            
            var weaponCoroutine = StartCoroutine(Type_Text(Weapons[i], Text_Seperator_0.Murder_Weapons[i]));
            Active_Coroutines.Add(weaponCoroutine);
            
            if (i < 2)
            {
                yield return new WaitForSeconds(Delay_Between_Words);
            }
        }
    }

    private IEnumerator Type_Text(TextMeshProUGUI tmp_Text, string full_Text)
    {
        tmp_Text.text = "";
        
        foreach (char c in full_Text)
        {
            tmp_Text.text += c;
            yield return new WaitForSeconds(Typing_Speed);
        }
    }

    private void Clear_All_Texts()
    {
        foreach (var tmp in Motives) tmp.text = "";
        foreach (var tmp in Locations) tmp.text = "";
        foreach (var tmp in Weapons) tmp.text = "";
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}