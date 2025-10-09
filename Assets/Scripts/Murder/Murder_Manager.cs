using UnityEngine;
using System.Text;
using System.Collections.Generic;
using TMPro;

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
    [SerializeField] private TMP_Text Suspect_Response_Display;
    [Space]
    [Header("Text Separators --------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;
    [SerializeField] private Text_Seperator_2 Text_Seperator_2;
    [Space]
    [Header("Suspect Configuration --------------------------------------------------------------")]
    [Space]
    [SerializeField] private Suspect_Type Current_Suspect_Type = Suspect_Type.Zeki;
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
    [Space]
    [Header("Debug Options -----------------------------------------------------------------------")]
    [SerializeField] private bool Enable_Debug_Keys = true;

    #endregion

    #region Private Vars
    private bool Waiting_For_Suspect_Response = false;
    private bool Game_Loop_Ready = false;
    private int Question_Count = 0;
    private const int MAX_QUESTIONS_FOR_SUMMARY = 5;
    private List<string> Early_Questions = new List<string>();
    private List<string> Early_Responses = new List<string>();
    #endregion

    #region Enums
    public enum Suspect_Type
    {
        Saf,
        Zeki,
        Psikopat
    }
    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Unity Lifecycle

    void Update()
    {
        if (!Game_Loop_Ready)
            return;

        // Wait for suspect response
        if (Waiting_For_Suspect_Response && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            Process_Suspect_Response();
            Waiting_For_Suspect_Response = false;
        }

        // Debug key for testing
        if (Enable_Debug_Keys && Input.GetKeyDown(KeyCode.Q) && !Gemini_Api_Handler.Is_Request_In_Progress)
        {
            Ask_Suspect_Question("Test sorusu: Dün gece neredeydin?");
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Public Methods

    public void Initialize_Suspect_System()
    {
        Game_Loop_Ready = true;
        Update_Emotional_State_Display();
        Debug.Log($"Murder Manager initialized. Suspect Type: {Current_Suspect_Type}");
    }

    public void On_Ask_Button_Pressed()
    {
        if (!Game_Loop_Ready)
        {
            Debug.LogWarning("Murder Manager is not ready yet!");
            return;
        }

        if (Gemini_Api_Handler.Is_Request_In_Progress)
        {
            Debug.LogWarning("Already waiting for a response!");
            return;
        }

        string player_question = Player_Input_Field.text.Trim();

        if (string.IsNullOrEmpty(player_question))
        {
            Debug.LogWarning("Player input is empty!");
            return;
        }

        Ask_Suspect_Question(player_question);
        Player_Input_Field.text = ""; // Clear input field
    }

    public void Ask_Suspect_Question(string player_question)
    {
        if (!Game_Loop_Ready || Waiting_For_Suspect_Response)
            return;

        Question_Count++;
        
        // Store early questions for summary
        if (Question_Count <= MAX_QUESTIONS_FOR_SUMMARY)
        {
            Early_Questions.Add(player_question);
        }

        Waiting_For_Suspect_Response = true;
        Send_Suspect_Prompt(player_question);
    }

    public void Change_Suspect_Type(int type_index)
    {
        Current_Suspect_Type = (Suspect_Type)type_index;
        Debug.Log($"Suspect type changed to: {Current_Suspect_Type}");
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Private Methods

    private void Send_Suspect_Prompt(string player_question)
    {
        if (Prompt_List_SO.Prompt_List.Count > 3)
        {
            string Base_Prompt = Prompt_List_SO.Prompt_List[3].Paragraph;
            string Full_Prompt = Build_Suspect_Prompt(Base_Prompt, player_question);
            Gemini_Api_Handler.Send_Prompt(Full_Prompt);
        }
        else
        {
            Debug.LogError("Prompt index 3 does not exist in Prompt_List_SO!");
        }
    }

    private void Process_Suspect_Response()
    {
        string response = Gemini_Api_Handler.Last_Response;
        
        // Parse the response
        Text_Seperator_2.Parse_Suspect_Response(response);
        
        // Display suspect response (NEW!)
        Display_Suspect_Response();
        
        // Update conversation history
        Update_Conversation_History(Text_Seperator_2.Suspect_Response);
        
        // Store early responses for summary
        if (Question_Count <= MAX_QUESTIONS_FOR_SUMMARY)
        {
            Early_Responses.Add(Text_Seperator_2.Suspect_Response);
        }
        
        // Generate summary if we reached the threshold
        if (Question_Count == MAX_QUESTIONS_FOR_SUMMARY)
        {
            Generate_Conversation_Summary();
        }
        
        // Apply emotion changes
        Apply_Emotion_Changes();
        
        // Update UI
        Update_Emotional_State_Display();
        
        Debug.Log($"Question {Question_Count} processed. Current emotions - Korku:{Korku}, Stres:{Stres}");
    }

    private string Build_Suspect_Prompt(string Base_Prompt, string player_question)
    {
        StringBuilder sb = new StringBuilder(Base_Prompt);
        sb.Append("\n\n");
        
        // === SUSPECT INFO ===
        sb.Append("#Katil_Bilgileri\n");
        sb.Append($"Katil_Tipi: {Current_Suspect_Type}\n");
        sb.Append($"Cinayet_Nedeni: {Text_Seperator_0.Motives_For_Murder[0]}\n");
        sb.Append($"Cinayet_Yeri: {Text_Seperator_0.Murder_Locations[0]}\n");
        sb.Append($"Cinayet_Silahı: {Text_Seperator_0.Murder_Weapons[0]}\n");
        sb.Append("\n");
        
        // === CONVERSATION SUMMARY ===
        sb.Append("#Konuşma_Özeti\n");
        if (!string.IsNullOrEmpty(Conversation_Summary))
        {
            sb.Append(Conversation_Summary).Append("\n");
        }
        else
        {
            sb.Append("İlk sorgulama. Henüz geçmiş yok.\n");
        }
        sb.Append("\n");
        
        // === LAST 3 MESSAGES ===
        sb.Append("#Son_3_Mesaj\n");
        if (!string.IsNullOrEmpty(Last_AI_Message_1))
        {
            sb.Append("AI: ").Append(Last_AI_Message_1).Append("\n");
            if (!string.IsNullOrEmpty(Last_Player_Message_1))
                sb.Append("Oyuncu: ").Append(Last_Player_Message_1).Append("\n");
        }
        if (!string.IsNullOrEmpty(Last_AI_Message_2))
        {
            sb.Append("AI: ").Append(Last_AI_Message_2).Append("\n");
            if (!string.IsNullOrEmpty(Last_Player_Message_2))
                sb.Append("Oyuncu: ").Append(Last_Player_Message_2).Append("\n");
        }
        if (!string.IsNullOrEmpty(Last_AI_Message_3))
        {
            sb.Append("AI: ").Append(Last_AI_Message_3).Append("\n");
        }
        sb.Append("\n");
        
        // === CURRENT EMOTIONS ===
        sb.Append("#Mevcut_Duygular\n");
        sb.Append($"Korku: {Korku}\n");
        sb.Append($"Stres: {Stres}\n");
        sb.Append($"Öfke: {Ofke}\n");
        sb.Append($"Soğukkanlılık: {Sogukkkanlilik}\n");
        sb.Append($"Yorgunluk: {Yorgunluk}\n");
        sb.Append("\n");
        
        // === NEW QUESTION ===
        sb.Append("#Yeni_Soru\n");
        sb.Append($"Oyuncu: {player_question}\n");
        
        return sb.ToString();
    }

    private void Update_Conversation_History(string ai_response)
    {
        // Shift messages (sliding window)
        Last_AI_Message_1 = Last_AI_Message_2;
        Last_Player_Message_1 = Last_Player_Message_2;
        Last_AI_Message_2 = Last_AI_Message_3;
        Last_Player_Message_2 = Player_Input_Field.text;
        Last_AI_Message_3 = ai_response;
    }

    private void Generate_Conversation_Summary()
    {
        StringBuilder summary = new StringBuilder();
        summary.Append($"İlk {MAX_QUESTIONS_FOR_SUMMARY} soruda:\n");
        
        // Count aggressive questions
        int aggressive_count = 0;
        int gentle_count = 0;
        
        foreach (string question in Early_Questions)
        {
            string lower = question.ToLowerInvariant();
            if (lower.Contains("yalan") || lower.Contains("suçlu") || lower.Contains("katil") || 
                lower.Contains("!") || lower.Contains("itiraf"))
            {
                aggressive_count++;
            }
            else if (lower.Contains("lütfen") || lower.Contains("yardım") || lower.Contains("anlat"))
            {
                gentle_count++;
            }
        }
        
        if (aggressive_count > gentle_count)
            summary.Append("- Oyuncu agresif davrandı\n");
        else if (gentle_count > aggressive_count)
            summary.Append("- Oyuncu nazik davrandı\n");
        else
            summary.Append("- Oyuncu dengeli davrandı\n");
        
        // Check emotion trends
        if (Korku > 60)
            summary.Append($"- Katil {MAX_QUESTIONS_FOR_SUMMARY - 1} kez korktu\n");
        if (Stres > 60)
            summary.Append("- Katil stres altında\n");
        if (Ofke > 40)
            summary.Append("- Katil sinirlendi\n");
        
        Conversation_Summary = summary.ToString();
    }

    private void Apply_Emotion_Changes()
    {
        Korku = Mathf.Clamp(Korku + Text_Seperator_2.Korku_Change, 0, 100);
        Stres = Mathf.Clamp(Stres + Text_Seperator_2.Stres_Change, 0, 100);
        Ofke = Mathf.Clamp(Ofke + Text_Seperator_2.Ofke_Change, 0, 100);
        Sogukkkanlilik = Mathf.Clamp(Sogukkkanlilik + Text_Seperator_2.Sogukkkanlilik_Change, 0, 100);
        Yorgunluk = Mathf.Clamp(Yorgunluk + Text_Seperator_2.Yorgunluk_Change, 0, 100);
        
        // Check critical states
        Check_Critical_Emotional_States();
    }

    private void Check_Critical_Emotional_States()
    {
        if (Korku >= 100)
        {
            Debug.LogWarning("⚠️ KORKU 100! Katil ağlayıp susuyor - OYUN BİTTİ!");
            // TODO: Trigger game over
        }
        else if (Stres >= 100)
        {
            Debug.LogWarning("⚠️ STRES 100! Katil öfke patlaması yaşıyor!");
            // TODO: Trigger aggressive state
        }
        else if (Yorgunluk >= 100)
        {
            Debug.LogWarning("⚠️ YORGUNLUK 100! Katil uyuya kalıyor - OYUN BİTTİ!");
            // TODO: Trigger game over
        }
    }

    private void Update_Emotional_State_Display()
    {
        if (Emotional_State_Display != null)
        {
            StringBuilder state_text = new StringBuilder();
            state_text.AppendLine($"<b>DUYGU DURUMU</b>");
            state_text.AppendLine($"<color=yellow>Korku:</color> {Korku}/100");
            state_text.AppendLine($"<color=red>Stres:</color> {Stres}/100");
            state_text.AppendLine($"<color=orange>Öfke:</color> {Ofke}/100");
            state_text.AppendLine($"<color=purple>Soğukkanlılık:</color> {Sogukkkanlilik}/100");
            state_text.AppendLine($"<color=green>Yorgunluk:</color> {Yorgunluk}/100");
            state_text.AppendLine();
            
            if (!string.IsNullOrEmpty(Text_Seperator_2.Current_Emotional_State))
            {
                state_text.AppendLine($"<b>Durum:</b> <i>{Text_Seperator_2.Current_Emotional_State}</i>");
            }
            
            Emotional_State_Display.text = state_text.ToString();
        }
    }

    private void Display_Suspect_Response()
    {
        if (Suspect_Response_Display != null)
        {
            // SADECE KATIL YANITI - TEMİZ
            Suspect_Response_Display.text = Text_Seperator_2.Suspect_Response;
        }
        else
        {
            Debug.LogWarning("Suspect_Response_Display is not assigned!");
            Debug.Log($"KATIL YANITI: {Text_Seperator_2.Suspect_Response}");
        }
    }

    [ContextMenu("Reset Emotions")]
    private void Reset_Emotions()
    {
        Korku = 40;
        Stres = 40;
        Ofke = 20;
        Sogukkkanlilik = 50;
        Yorgunluk = 10;
        Update_Emotional_State_Display();
    }

    [ContextMenu("Debug Print State")]
    private void Debug_Print_State()
    {
        Debug.Log("=== MURDER MANAGER STATE ===");
        Debug.Log($"Suspect Type: {Current_Suspect_Type}");
        Debug.Log($"Question Count: {Question_Count}");
        Debug.Log($"Emotions - K:{Korku} S:{Stres} O:{Ofke} Sg:{Sogukkkanlilik} Y:{Yorgunluk}");
        Debug.Log($"Summary: {Conversation_Summary}");
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}