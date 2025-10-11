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
    [Header("Game State -------------------------------------------------------------------------")]
    [Space]
    public int Question_Count = 0;
    public int Max_Questions = 10;
    [Space]
    [Header("Debug Options -----------------------------------------------------------------------")]
    [SerializeField] private bool Enable_Debug_Keys = true;
    [SerializeField] private bool Show_Detailed_Logs = false;

    #endregion

    #region Private Vars
        private Coroutine Typewriter_Coroutine;
    private bool Waiting_For_Suspect_Response = false;
    private bool Game_Loop_Ready = false;
    private const int MAX_QUESTIONS_FOR_SUMMARY = 5;
    private List<string> Early_Questions = new List<string>();
    private List<string> Early_Responses = new List<string>();
    private string Last_Player_Question = "";
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

        // Debug key
        if (Enable_Debug_Keys && Input.GetKeyDown(KeyCode.Q) && !Gemini_Api_Handler.Is_Request_In_Progress)
        {
            Ask_Suspect_Question("Test: Dün gece neredeydin?");
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Public Methods

    public void Initialize_Suspect_System()
    {
        Game_Loop_Ready = true;
        Update_Emotional_State_Display();
        Debug.Log($"[Murder_Manager] Initialized. Questions: {Question_Count}/{Max_Questions}");
    }

    public void On_Ask_Button_Pressed()
    {
    if (!Game_Loop_Ready)
    {
        Debug.LogWarning("[Murder_Manager] System not ready!");
        return;
    }

    if (Gemini_Api_Handler.Is_Request_In_Progress)
    {
        Debug.LogWarning("[Murder_Manager] Already waiting for response!");
        return;
    }

    if (Question_Count >= Max_Questions)
    {
        Debug.LogWarning("[Murder_Manager] No more questions left!");
        return;
    }

    string player_question = Player_Input_Field.text.Trim();

    if (string.IsNullOrEmpty(player_question))
    {
        Debug.LogWarning("[Murder_Manager] Player input is empty!");
        return;
    }

    Clear_Suspect_Response();
    Ask_Suspect_Question(player_question);
    Player_Input_Field.text = "";
    }

    public void Clear_Suspect_Response()
    {
        if (Suspect_Response_Display == null)
        {
            Debug.LogWarning("[Murder_Manager] Suspect_Response_Display is not assigned!");
            return;
        }

        // Eğer aktif bir yazma coroutine'i varsa, önce onu durdur
        if (Typewriter_Coroutine != null)
        {
            StopCoroutine(Typewriter_Coroutine);
            Typewriter_Coroutine = null;
        }

        // Şu anki metni al ve silme animasyonunu başlat
        string currentText = Suspect_Response_Display.text;
        Typewriter_Coroutine = StartCoroutine(Typewriter_Effect_Minus(currentText, Suspect_Response_Display));
    }



    public void Ask_Suspect_Question(string player_question)
    {
        if (!Game_Loop_Ready || Waiting_For_Suspect_Response)
            return;

        Question_Count++;
        Last_Player_Question = player_question;

        // Store early questions for summary
        if (Question_Count <= MAX_QUESTIONS_FOR_SUMMARY)
        {
            Early_Questions.Add(player_question);
        }

        if (Show_Detailed_Logs)
            Debug.Log($"[Murder_Manager] Question {Question_Count}/{Max_Questions}: {player_question}");

        Waiting_For_Suspect_Response = true;
        Send_Suspect_Prompt(player_question);
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
            
            if (Show_Detailed_Logs)
                Debug.Log($"[Murder_Manager] Sending prompt. Length: {Full_Prompt.Length} chars");
            
            Gemini_Api_Handler.Send_Prompt(Full_Prompt);
        }
        else
        {
            Debug.LogError("[Murder_Manager] Prompt index 3 does not exist in Prompt_List_SO!");
        }
    }

    private void Process_Suspect_Response()
    {
        string response = Gemini_Api_Handler.Last_Response;
        
        if (Show_Detailed_Logs)
            Debug.Log($"[Murder_Manager] Received response. Length: {response.Length} chars");
        
        // Parse the response
        Text_Seperator_2.Parse_Suspect_Response(response);
        
        // Display suspect response
        Display_Suspect_Response();
        
        // Update conversation history
        Update_Conversation_History(Text_Seperator_2.Suspect_Response);
        
        // Store early responses for summary
        if (Question_Count <= MAX_QUESTIONS_FOR_SUMMARY)
        {
            Early_Responses.Add(Text_Seperator_2.Suspect_Response);
        }
        
        // Generate summary if threshold reached
        if (Question_Count == MAX_QUESTIONS_FOR_SUMMARY)
        {
            Generate_Conversation_Summary();
        }
        
        // Apply emotion changes
        Apply_Emotion_Changes();
        
        // Update UI
        Update_Emotional_State_Display();
        
        Debug.Log($"[Murder_Manager] Q{Question_Count} processed. Emotions: K:{Korku} S:{Stres} O:{Ofke} Sg:{Sogukkkanlilik} Y:{Yorgunluk}");
    }

    private string Build_Suspect_Prompt(string Base_Prompt, string player_question)
    {
        StringBuilder sb = new StringBuilder(Base_Prompt);
        sb.Append("\n\n");
        
        // === SUSPECT INFO ===
        sb.Append("#Katil_Bilgileri\n");
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
        // Sliding window: shift messages
        Last_AI_Message_1 = Last_AI_Message_2;
        Last_Player_Message_1 = Last_Player_Message_2;
        Last_AI_Message_2 = Last_AI_Message_3;
        Last_Player_Message_2 = Last_Player_Question;
        Last_AI_Message_3 = ai_response;
    }

    private void Generate_Conversation_Summary()
    {
        StringBuilder summary = new StringBuilder();
        summary.Append($"İlk {MAX_QUESTIONS_FOR_SUMMARY} soruda:\n");
        
        // Count question types
        int aggressive_count = 0;
        int gentle_count = 0;
        int evidence_count = 0;
        
        foreach (string question in Early_Questions)
        {
            string lower = question.ToLowerInvariant();
            
            // Aggressive indicators
            if (lower.Contains("yalan") || lower.Contains("suçlu") || lower.Contains("katil") || 
                lower.Contains("!") || lower.Contains("itiraf") || lower.Contains("hapis"))
            {
                aggressive_count++;
            }
            // Gentle indicators
            else if (lower.Contains("lütfen") || lower.Contains("yardım") || lower.Contains("anlat") ||
                     lower.Contains("sakin"))
            {
                gentle_count++;
            }
            // Evidence indicators
            if (lower.Contains("tanık") || lower.Contains("kanıt") || lower.Contains("gördü") ||
                lower.Contains("biliyorum"))
            {
                evidence_count++;
            }
        }
        
        // Player behavior summary
        if (aggressive_count > gentle_count)
            summary.Append("- Oyuncu agresif davrandı\n");
        else if (gentle_count > aggressive_count)
            summary.Append("- Oyuncu nazik davrandı\n");
        else
            summary.Append("- Oyuncu dengeli davrandı\n");
        
        if (evidence_count > 0)
            summary.Append($"- {evidence_count} kez kanıt/tanık bahsetti\n");
        
        // Emotion trends
        if (Korku > 60)
            summary.Append("- Katil çok korktu\n");
        if (Stres > 60)
            summary.Append("- Katil stres altında\n");
        if (Ofke > 40)
            summary.Append("- Katil sinirlendi\n");
        if (Sogukkkanlilik < 30)
            summary.Append("- Katil kontrolünü kaybediyor\n");
        
        Conversation_Summary = summary.ToString();
        
        if (Show_Detailed_Logs)
            Debug.Log($"[Murder_Manager] Summary generated:\n{Conversation_Summary}");
    }

    private void Apply_Emotion_Changes()
    {
        // Apply changes with clamping
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
        bool game_over = false;
        string reason = "";
        
        if (Korku >= 100)
        {
            game_over = true;
            reason = "Korku 100! Katil ağlayıp susuyor.";
        }
        else if (Stres >= 100)
        {
            game_over = true;
            reason = "Stres 100! Katil öfke patlaması yaşıyor.";
        }
        else if (Yorgunluk >= 100)
        {
            game_over = true;
            reason = "Yorgunluk 100! Katil uyuya kalıyor.";
        }
        
        if (game_over)
        {
            Debug.LogWarning($"[Murder_Manager] ⚠️ GAME OVER: {reason}");
            // TODO: Trigger game over scene
            Game_Loop_Ready = false;
        }
        else
        {
            // Log warnings for approaching critical states
            if (Korku >= 85)
                Debug.LogWarning("[Murder_Manager] ⚠️ Korku çok yüksek! (≥85)");
            if (Stres >= 85)
                Debug.LogWarning("[Murder_Manager] ⚠️ Stres çok yüksek! (≥85)");
            if (Yorgunluk >= 85)
                Debug.LogWarning("[Murder_Manager] ⚠️ Yorgunluk çok yüksek! (≥85)");
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
            
            // Add emotional state description
            if (!string.IsNullOrEmpty(Text_Seperator_2.Current_Emotional_State))
            {
                state_text.AppendLine($"<b>Durum:</b> <i>{Text_Seperator_2.Current_Emotional_State}</i>");
            }
            
            // Add question counter
            state_text.AppendLine();
            state_text.Append($"<b>Soru:</b> {Question_Count}/{Max_Questions}");
            
            Emotional_State_Display.text = state_text.ToString();
        }
    }

    private void Display_Suspect_Response()
    {
        if (Suspect_Response_Display != null)
        {
            if (Typewriter_Coroutine != null)
            StopCoroutine(Typewriter_Coroutine); // Önceki yazıyı iptal et

            Typewriter_Coroutine = StartCoroutine(
            Typewriter_Effect(Text_Seperator_2.Suspect_Response, Suspect_Response_Display)
            );
        }
        else
        {
            Debug.LogWarning("[Murder_Manager] Suspect_Response_Display not assigned!");
            Debug.Log($"KATIL: {Text_Seperator_2.Suspect_Response}");
        }
    }

    private System.Collections.IEnumerator Typewriter_Effect(string Text, TMP_Text Target_TMP)
    {
        Target_TMP.text = "";

        foreach (char c in Text)
        {
            Target_TMP.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        Typewriter_Coroutine = null;
    }
    
    private System.Collections.IEnumerator Typewriter_Effect_Minus(string Text, TMP_Text Target_TMP)
    {
        Target_TMP.text = Text;

        for (int i = Text.Length; i >= 0; i--)
        {
            Target_TMP.text = Text.Substring(0, i);
            yield return new WaitForSeconds(0.01f);
        }
        Typewriter_Coroutine = null;
    }


   
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}