using UnityEngine;

public class Game_Loop_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("Referances -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Gemini_Api_Handler Gemini_Api_Handler;
    [SerializeField] private GameObject Loading_Screen;
    [SerializeField] private Prompt_List_SO Prompt_List_SO;
    [Header("Targeted_Prompt --------------------------------------------------------------------")]
    [SerializeField] private string Targeted_Prompt;

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Unity_Lifecycle

    void Awake()
    {
        Loading_Screen.SetActive(true);
        Send_Prompt_From_List(0); //Başlangıç Promptunu Gönder
    }


    void Update()
    {
        if (!Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            Loading_Screen.SetActive(false);
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\    
    #region Public Methods

    public void Send_Prompt_From_List(int Prompt_Index)
    {
        if (Prompt_List_SO.Prompt_List.Count > 0)
        {
            Targeted_Prompt = Prompt_List_SO.Prompt_List[Prompt_Index].Paragraph;
            Gemini_Api_Handler.Send_Prompt(Targeted_Prompt);
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\    
}
