using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Text_Seperator_2 : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab
    
    [Header("Suspect Response Data -------------------------------------------------------------")]
    [Space]
    [TextArea(3, 10)]
    public string Suspect_Response = "";
    [Space]
    [Header("Emotion Changes --------------------------------------------------------------------")]
    [Space]
    public int Korku_Change = 0;
    public int Stres_Change = 0;
    public int Ofke_Change = 0;
    public int Sogukkkanlilik_Change = 0;
    public int Yorgunluk_Change = 0;
    [Space]
    [Header("Emotion Reasons --------------------------------------------------------------------")]
    [Space]
    [TextArea(2, 5)]
    public string Korku_Reason = "";
    [TextArea(2, 5)]
    public string Stres_Reason = "";
    [TextArea(2, 5)]
    public string Ofke_Reason = "";
    [TextArea(2, 5)]
    public string Sogukkkanlilik_Reason = "";
    [TextArea(2, 5)]
    public string Yorgunluk_Reason = "";
    [Space]
    [Header("Current State ----------------------------------------------------------------------")]
    [Space]
    [TextArea(1, 3)]
    public string Current_Emotional_State = "";

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    
    #region Public Methods

    public void Parse_Suspect_Response(string Response)
    {
        if (string.IsNullOrEmpty(Response))
        {
            Debug.LogError("Suspect response is empty!");
            return;
        }

        Clear_All_Data();

        string[] lines = Response.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToArray();

        string Current_Section = "";
        System.Text.StringBuilder Response_Builder = null;
        Dictionary<string, int> Emotion_Changes = new Dictionary<string, int>();
        Dictionary<string, string> Emotion_Reasons = new Dictionary<string, string>();

        foreach (string line in lines)
        {
            if (line.StartsWith("#"))
            {
                // Save previous section
                if (Response_Builder != null)
                {
                    Suspect_Response = Response_Builder.ToString().Trim();
                    Response_Builder = null;
                }

                Current_Section = line.Trim();

                // Start building response text
                if (Current_Section == "#Yanıt")
                {
                    Response_Builder = new System.Text.StringBuilder();
                }
            }
            else
            {
                if (Response_Builder != null)
                {
                    // Building suspect's dialogue
                    if (Response_Builder.Length > 0)
                        Response_Builder.AppendLine();
                    Response_Builder.Append(line);
                }
                else if (Current_Section == "#Duygu_Değişimleri")
                {
                    // Parse emotion changes
                    Parse_Emotion_Change(line, Emotion_Changes);
                }
                else if (Current_Section == "#Gerekçe")
                {
                    // Parse emotion reasons
                    Parse_Emotion_Reason(line, Emotion_Reasons);
                }
                else if (Current_Section == "#Durum")
                {
                    // Parse emotional state
                    Current_Emotional_State = line.Replace("*", "").Trim();
                }
            }
        }

        // Save last response if exists
        if (Response_Builder != null)
        {
            Suspect_Response = Response_Builder.ToString().Trim();
        }

        // Apply parsed emotion changes
        Apply_Emotion_Changes(Emotion_Changes);
        Apply_Emotion_Reasons(Emotion_Reasons);
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Private Methods

    private void Clear_All_Data()
    {
        Suspect_Response = "";
        Korku_Change = 0;
        Stres_Change = 0;
        Ofke_Change = 0;
        Sogukkkanlilik_Change = 0;
        Yorgunluk_Change = 0;
        Korku_Reason = "";
        Stres_Reason = "";
        Ofke_Reason = "";
        Sogukkkanlilik_Reason = "";
        Yorgunluk_Reason = "";
        Current_Emotional_State = "";
    }

    private void Parse_Emotion_Change(string line, Dictionary<string, int> Emotion_Changes)
    {
        // Format: "Korku: +15" or "Stres: -10"
        string[] parts = line.Split(':');
        if (parts.Length == 2)
        {
            string emotion = parts[0].Trim();
            string value_str = parts[1].Trim();
            
            // Parse integer value
            if (int.TryParse(value_str, out int value))
            {
                Emotion_Changes[emotion] = value;
            }
            else
            {
                Debug.LogWarning($"Could not parse emotion value: {line}");
            }
        }
    }

    private void Parse_Emotion_Reason(string line, Dictionary<string, string> Emotion_Reasons)
    {
        // Format: "Korku: Tanık var, tehlike arttı"
        string[] parts = line.Split(new[] { ':' }, 2);
        if (parts.Length == 2)
        {
            string emotion = parts[0].Trim();
            string reason = parts[1].Trim();
            Emotion_Reasons[emotion] = reason;
        }
    }

    private void Apply_Emotion_Changes(Dictionary<string, int> Emotion_Changes)
    {
        if (Emotion_Changes.ContainsKey("Korku"))
            Korku_Change = Emotion_Changes["Korku"];
        
        if (Emotion_Changes.ContainsKey("Stres"))
            Stres_Change = Emotion_Changes["Stres"];
        
        if (Emotion_Changes.ContainsKey("Öfke"))
            Ofke_Change = Emotion_Changes["Öfke"];
        
        if (Emotion_Changes.ContainsKey("Soğukkanlılık"))
            Sogukkkanlilik_Change = Emotion_Changes["Soğukkanlılık"];
        
        if (Emotion_Changes.ContainsKey("Yorgunluk"))
            Yorgunluk_Change = Emotion_Changes["Yorgunluk"];
    }

    private void Apply_Emotion_Reasons(Dictionary<string, string> Emotion_Reasons)
    {
        if (Emotion_Reasons.ContainsKey("Korku"))
            Korku_Reason = Emotion_Reasons["Korku"];
        
        if (Emotion_Reasons.ContainsKey("Stres"))
            Stres_Reason = Emotion_Reasons["Stres"];
        
        if (Emotion_Reasons.ContainsKey("Öfke"))
            Ofke_Reason = Emotion_Reasons["Öfke"];
        
        if (Emotion_Reasons.ContainsKey("Soğukkanlılık"))
            Sogukkkanlilik_Reason = Emotion_Reasons["Soğukkanlılık"];
        
        if (Emotion_Reasons.ContainsKey("Yorgunluk"))
            Yorgunluk_Reason = Emotion_Reasons["Yorgunluk"];
    }

    [ContextMenu("Debug Print Suspect Response")]
    private void Debug_Print_Results()
    {
        Debug.Log("=== SUSPECT RESPONSE ===");
        Debug.Log($"Response: {Suspect_Response}");
        Debug.Log("\n=== EMOTION CHANGES ===");
        Debug.Log($"Korku: {Korku_Change} ({Korku_Reason})");
        Debug.Log($"Stres: {Stres_Change} ({Stres_Reason})");
        Debug.Log($"Öfke: {Ofke_Change} ({Ofke_Reason})");
        Debug.Log($"Soğukkanlılık: {Sogukkkanlilik_Change} ({Sogukkkanlilik_Reason})");
        Debug.Log($"Yorgunluk: {Yorgunluk_Change} ({Yorgunluk_Reason})");
        Debug.Log($"\n=== CURRENT STATE ===");
        Debug.Log($"State: {Current_Emotional_State}");
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}