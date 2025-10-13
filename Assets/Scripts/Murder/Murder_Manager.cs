using UnityEngine;
using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Murder_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("References -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Gemini_Api_Handler Gemini_Api_Handler;
    [SerializeField] private Prompt_List_SO Prompt_List_SO;
    [SerializeField] private TMP_InputField Player_Input_Field;
    [Space]
    [Header("Display References --------------------------------------------------------------")]
    [Space]
    [SerializeField] private TMP_Text Emotional_State_Display;
    [SerializeField] public TMP_Text Suspect_Response_Display;
    [Space]
    [Header("Text Separators --------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;
    [SerializeField] private Text_Seperator_2 Text_Seperator_2;
    [Space]
    [Header("Button References -------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Button Ask_Suspect_Button;
    [SerializeField] private Button Suspicion_Words_Button;
    [SerializeField] private Button Witness_Button;
    [Space]
    [Header("Current Emotions (Public Int) ------------------------------------------------------")]
    [Space]
    public int Korku = 40;
    public int Stres = 40;
    public int Ofke = 20;
    public int Sogukkkanlilik = 50;
    public int Yorgunluk = 10;
    [Space]
    [Header("Conversation History ---------------------------------------------------------------")]
    [Space]
    [TextArea(3, 10)]
    public string Conversation_Summary = "";
    [Space]
    [TextArea(2, 5)]
    public string Last_AI_Message_1 = "";
    [TextArea(2, 5)]
    public string Last_Player_Message_1 = "";
    [TextArea(2, 5)]
    public string Last_AI_Message_2 = "";
    [TextArea(2, 5)]
    public string Last_Player_Message_2 = "";
    [TextArea(2, 5)]
    public string Last_AI_Message_3 = "";

    #endregion

    #region Private Vars
    public Coroutine Typewriter_Coroutine;
    private bool Waiting_For_Suspect_Response = false;
    private bool Game_Loop_Ready = false;
    private const int SUMMARY_THRESHOLD = 5;
    private int Question_Count = 0;
    private List<string> Early_Questions = new List<string>();
    private string Last_Player_Question = "";
    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Unity Lifecycle

    void Update()
    {
        if (!Game_Loop_Ready) return;

        if (Waiting_For_Suspect_Response && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            Process_Suspect_Response();
            Waiting_For_Suspect_Response = false;
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Public Methods

    public void Initialize_Suspect_System()
    {
        Game_Loop_Ready = true;
        Update_Emotional_State_Display();
    }

    public void On_Ask_Button_Pressed()
    {
        if (!Game_Loop_Ready || Gemini_Api_Handler.Is_Request_In_Progress) return;

        string player_question = Player_Input_Field.text.Trim();
        if (string.IsNullOrEmpty(player_question)) return;
    
        // Disable buttons
        Set_Buttons_Interactable(false);
        
        // Clear and ask
        Clear_Suspect_Response();
        Ask_Suspect_Question(player_question);
        Player_Input_Field.text = "";
    }

    public void Ask_Suspect_Question(string player_question)
    {
        if (!Game_Loop_Ready || Waiting_For_Suspect_Response) return;

        Question_Count++;
        Last_Player_Question = player_question;
        
        if (Question_Count <= SUMMARY_THRESHOLD)
            Early_Questions.Add(player_question);

        Waiting_For_Suspect_Response = true;
        Send_Suspect_Prompt(player_question);
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Private Methods

    private void Send_Suspect_Prompt(string player_question)
    {
        if (Prompt_List_SO.Prompt_List.Count <= 3)
        {
            Debug.LogError("[Murder_Manager] Prompt index 3 missing!");
            return;
        }

        string prompt = Build_Suspect_Prompt(Prompt_List_SO.Prompt_List[3].Paragraph, player_question);
        Gemini_Api_Handler.Send_Prompt(prompt);
    }

    private void Process_Suspect_Response()
    {
        // Parse
        Text_Seperator_2.Parse_Suspect_Response(Gemini_Api_Handler.Last_Response);
        
        // Display
        Display_Suspect_Response();
        
        // Update history
        Update_Conversation_History(Text_Seperator_2.Suspect_Response);
        
        // Generate summary if needed
        if (Question_Count == SUMMARY_THRESHOLD)
            Generate_Conversation_Summary();
        
        // Apply emotions
        Apply_Emotion_Changes();
        
        // Update UI
        Update_Emotional_State_Display();
    }

    private string Build_Suspect_Prompt(string base_prompt, string player_question)
    {
        StringBuilder sb = new StringBuilder(base_prompt);
        sb.Append("\n\n");
        
        sb.Append("#Katil_Bilgileri\n");
        sb.Append($"Cinayet_Nedeni: {Text_Seperator_0.Motives_For_Murder[0]}\n");
        sb.Append($"Cinayet_Yeri: {Text_Seperator_0.Murder_Locations[0]}\n");
        sb.Append($"Cinayet_Silahı: {Text_Seperator_0.Murder_Weapons[0]}\n\n");
        
        sb.Append("#Konuşma_Özeti\n");
        sb.Append(string.IsNullOrEmpty(Conversation_Summary) ? "İlk sorgulama\n" : Conversation_Summary);
        sb.Append("\n");
        
        sb.Append("#Son_3_Mesaj\n");
        if (!string.IsNullOrEmpty(Last_AI_Message_1))
        {
            sb.Append($"AI: {Last_AI_Message_1}\n");
            if (!string.IsNullOrEmpty(Last_Player_Message_1))
                sb.Append($"Oyuncu: {Last_Player_Message_1}\n");
        }
        if (!string.IsNullOrEmpty(Last_AI_Message_2))
        {
            sb.Append($"AI: {Last_AI_Message_2}\n");
            if (!string.IsNullOrEmpty(Last_Player_Message_2))
                sb.Append($"Oyuncu: {Last_Player_Message_2}\n");
        }
        if (!string.IsNullOrEmpty(Last_AI_Message_3))
            sb.Append($"AI: {Last_AI_Message_3}\n");
        sb.Append("\n");
        
        sb.Append("#Mevcut_Duygular\n");
        sb.Append($"Korku: {Korku}\n");
        sb.Append($"Stres: {Stres}\n");
        sb.Append($"Öfke: {Ofke}\n");
        sb.Append($"Soğukkanlılık: {Sogukkkanlilik}\n");
        sb.Append($"Yorgunluk: {Yorgunluk}\n\n");
        
        sb.Append("#Yeni_Soru\n");
        sb.Append($"Oyuncu: {player_question}\n");
        
        return sb.ToString();
    }

    private void Update_Conversation_History(string ai_response)
    {
        Last_AI_Message_1 = Last_AI_Message_2;
        Last_Player_Message_1 = Last_Player_Message_2;
        Last_AI_Message_2 = Last_AI_Message_3;
        Last_Player_Message_2 = Last_Player_Question;
        Last_AI_Message_3 = ai_response;
    }

    private void Generate_Conversation_Summary()
    {
        StringBuilder sb = new StringBuilder($"İlk {SUMMARY_THRESHOLD} soruda:\n");
        
        int aggressive = 0, gentle = 0, evidence = 0;
        
        foreach (string q in Early_Questions)
        {
            string lower = q.ToLowerInvariant();
            if (lower.Contains("yalan") || lower.Contains("suçlu") || lower.Contains("katil") || lower.Contains("!"))
                aggressive++;
            else if (lower.Contains("lütfen") || lower.Contains("yardım") || lower.Contains("sakin"))
                gentle++;
            if (lower.Contains("tanık") || lower.Contains("kanıt") || lower.Contains("gördü"))
                evidence++;
        }
        
        if (aggressive > gentle)
            sb.Append("- Oyuncu agresif\n");
        else if (gentle > aggressive)
            sb.Append("- Oyuncu nazik\n");
        else
            sb.Append("- Oyuncu dengeli\n");
        
        if (evidence > 0)
            sb.Append($"- {evidence} kez kanıt bahsetti\n");
        if (Korku > 60)
            sb.Append("- Katil çok korktu\n");
        if (Stres > 60)
            sb.Append("- Katil stresli\n");
        if (Ofke > 40)
            sb.Append("- Katil sinirli\n");
        
        Conversation_Summary = sb.ToString();
    }

    private void Apply_Emotion_Changes()
    {
        Korku = Mathf.Clamp(Korku + Text_Seperator_2.Korku_Change, 0, 100);
        Stres = Mathf.Clamp(Stres + Text_Seperator_2.Stres_Change, 0, 100);
        Ofke = Mathf.Clamp(Ofke + Text_Seperator_2.Ofke_Change, 0, 100);
        Sogukkkanlilik = Mathf.Clamp(Sogukkkanlilik + Text_Seperator_2.Sogukkkanlilik_Change, 0, 100);
        Yorgunluk = Mathf.Clamp(Yorgunluk + Text_Seperator_2.Yorgunluk_Change, 0, 100);
    }

    private void Update_Emotional_State_Display()
    {
        if (Emotional_State_Display == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>DUYGU DURUMU</b>");
        sb.AppendLine($"<color=yellow>Korku:</color> {Korku}/100");
        sb.AppendLine($"<color=red>Stres:</color> {Stres}/100");
        sb.AppendLine($"<color=orange>Öfke:</color> {Ofke}/100");
        sb.AppendLine($"<color=cyan>Soğukkanlılık:</color> {Sogukkkanlilik}/100");
        sb.AppendLine($"<color=gray>Yorgunluk:</color> {Yorgunluk}/100");
        
        if (!string.IsNullOrEmpty(Text_Seperator_2.Current_Emotional_State))
        {
            sb.AppendLine();
            sb.Append($"<b>Durum:</b> <i>{Text_Seperator_2.Current_Emotional_State}</i>");
        }
        
        Emotional_State_Display.text = sb.ToString();
    }

    private void Display_Suspect_Response()
    {
        if (Suspect_Response_Display == null) return;

        if (Typewriter_Coroutine != null)
            StopCoroutine(Typewriter_Coroutine);

        Typewriter_Coroutine = StartCoroutine(
            Typewriter_Effect(Text_Seperator_2.Suspect_Response, Suspect_Response_Display)
        );
    }
    
    private void Clear_Suspect_Response()
    {
        if (Suspect_Response_Display == null) return;

        if (Typewriter_Coroutine != null)
        {
            StopCoroutine(Typewriter_Coroutine);
            Typewriter_Coroutine = null;
        }

        Typewriter_Coroutine = StartCoroutine(
            Typewriter_Effect_Minus(Suspect_Response_Display.text, Suspect_Response_Display)
        );
    }

    private void Set_Buttons_Interactable(bool state)
    {
        if (Witness_Button != null) Witness_Button.interactable = state;
        if (Ask_Suspect_Button != null) Ask_Suspect_Button.interactable = state;
        if (Suspicion_Words_Button != null) Suspicion_Words_Button.interactable = state;
    }

    private System.Collections.IEnumerator Typewriter_Effect(string text, TMP_Text target)
    {
        target.text = "";

        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        
        Typewriter_Coroutine = null;
        Set_Buttons_Interactable(true);
    }
    
    private System.Collections.IEnumerator Typewriter_Effect_Minus(string text, TMP_Text target)
    {
        for (int i = text.Length; i >= 0; i--)
        {
            target.text = text.Substring(0, i);
            yield return new WaitForSeconds(0.01f);
        }
        
        Typewriter_Coroutine = null;
    }

    [ContextMenu("Reset All")]
    private void Reset_All()
    {
        Korku = 40;
        Stres = 40;
        Ofke = 20;
        Sogukkkanlilik = 50;
        Yorgunluk = 10;
        Question_Count = 0;
        Conversation_Summary = "";
        Last_AI_Message_1 = "";
        Last_Player_Message_1 = "";
        Last_AI_Message_2 = "";
        Last_Player_Message_2 = "";
        Last_AI_Message_3 = "";
        Early_Questions.Clear();
        Update_Emotional_State_Display();
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}