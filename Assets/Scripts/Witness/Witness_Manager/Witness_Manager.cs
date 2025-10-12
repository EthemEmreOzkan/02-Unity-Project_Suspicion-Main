using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class Witness_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("References -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Gemini_Api_Handler Gemini_Api_Handler;
    [SerializeField] private Prompt_List_SO Prompt_List_SO;
    [Space]
    [Header("Text Separators --------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;
    [SerializeField] private Text_Seperator_1 Text_Seperator_1;
    [Space]
    [Header("Button References -------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Button Ask_Suspect_Button;
    [SerializeField] private Button Suspicion_Words_Button;
    [SerializeField] private Button Witness_Button;
    [Space]

    #endregion

    #region Private Vars
    private bool Witness_Interrogation_Started = false;
    private bool Witness_Interrogation_Completed = false;
    private bool Waiting_For_Witness_Response = false;
    private bool Game_Loop_Ready = false;
    private string Targeted_Prompt;
    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Unity Lifecycle

    void Update()
    {
        if (!Game_Loop_Ready)
            return;

        if (!Witness_Interrogation_Started && !Witness_Interrogation_Completed && !Gemini_Api_Handler.Is_Request_In_Progress)
        {
            Witness_Interrogation_Started = true;
            Waiting_For_Witness_Response = true;
            Start_Witness_Interrogation();

        }
        
        if (Waiting_For_Witness_Response && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            Process_Witness_Response();
            Waiting_For_Witness_Response = false;
            
            Witness_Button.interactable = true;
            Ask_Suspect_Button.interactable = true;
            Suspicion_Words_Button.interactable = true;

            if (Witness_Interrogation_Started)
            {
                Witness_Interrogation_Started = false;
                Witness_Interrogation_Completed = true;
            }
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Public Methods

    public void Initialize_Witness_System()
    {
        Game_Loop_Ready = true;
    }

    public void Start_Witness_Interrogation()
    {
        Text_Seperator_1.Save_Previous_Response();
        Text_Seperator_1.Start_Waiting_Animation();
        Send_Witness_Prompt();

        Witness_Button.interactable = false;
        Ask_Suspect_Button.interactable = false;
        Suspicion_Words_Button.interactable = false;
    }

    public void Manual_Witness_Interrogation()
    {
        if (!Game_Loop_Ready || Gemini_Api_Handler.Is_Request_In_Progress || Waiting_For_Witness_Response)
            return;
        
        Waiting_For_Witness_Response = true;
        Text_Seperator_1.Save_Previous_Response();
        Text_Seperator_1.Start_Waiting_Animation();
        Send_Witness_Prompt();

        Witness_Button.interactable = false;
        Ask_Suspect_Button.interactable = false;
        Suspicion_Words_Button.interactable = false;
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Private Methods

    private void Send_Witness_Prompt()
    {
        if (Prompt_List_SO.Prompt_List.Count > 1)
        {
            string Base_Prompt = Prompt_List_SO.Prompt_List[1].Paragraph;
            Targeted_Prompt = Build_Witness_Prompt(Base_Prompt);
            Gemini_Api_Handler.Send_Prompt(Targeted_Prompt);
        }
    }

    private void Process_Witness_Response()
    {
        Text_Seperator_1.Parse_Response_Witness(Gemini_Api_Handler.Last_Response);
    }

    private string Build_Witness_Prompt(string Base_Prompt)
    {
        StringBuilder sb = new StringBuilder(Base_Prompt);
        sb.Append("\n\n");
        sb.Append("Önceki İfade: ").Append(Text_Seperator_1.Previous_Witness_Response).Append("\n");
        sb.Append("Kelimeler: ");
        sb.Append(Text_Seperator_0.Motives_For_Murder[0]).Append(", ");
        sb.Append(Text_Seperator_0.Motives_For_Murder[1]).Append(", ");
        sb.Append(Text_Seperator_0.Motives_For_Murder[2]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Locations[0]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Locations[1]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Locations[2]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Weapons[0]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Weapons[1]).Append(", ");
        sb.Append(Text_Seperator_0.Murder_Weapons[2]).Append("\n");
        sb.Append("Katil Hikayesi: ").Append(Text_Seperator_0.Murder_Scenario);
        
        return sb.ToString();
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}