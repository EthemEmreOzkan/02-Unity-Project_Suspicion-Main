using UnityEngine;

public class Game_Loop_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("References -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Gemini_Api_Handler Gemini_Api_Handler;
    [SerializeField] private GameObject Loading_Screen;
    [SerializeField] private Prompt_List_SO Prompt_List_SO;
    [SerializeField] private Witness_Manager Witness_Manager;
    [Space]
    [Header("Text Separators --------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;

    #endregion

    #region Private Vars
    private bool Initial_Response_Processed = false;
    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Unity Lifecycle

    void Awake()
    {
        Before_Loading_Screen();
    }

    void Update()
    {
        if (!Initial_Response_Processed && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            After_Loading_Screen();
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\    

    #region Public Methods

    public void Send_Prompt_From_List(int Prompt_Index)
    {
        if (Prompt_List_SO.Prompt_List.Count > Prompt_Index)
        {
            if (Prompt_Index == 0)
            {
                string Targeted_Prompt = Prompt_List_SO.Prompt_List[Prompt_Index].Paragraph;
                Gemini_Api_Handler.Send_Prompt(Targeted_Prompt);
            }
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    #region Private Methods

    private void Before_Loading_Screen()
    {
        Loading_Screen.SetActive(true);
        Send_Prompt_From_List(0);
    }

    private void After_Loading_Screen()
    {
        Loading_Screen.SetActive(false);
        Text_Seperator_0.Parse_Response(Gemini_Api_Handler.Last_Response);
        Initial_Response_Processed = true;
        Witness_Manager.Initialize_Witness_System();
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}