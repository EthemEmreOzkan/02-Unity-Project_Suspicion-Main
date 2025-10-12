using System.Collections;
using TMPro;
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
    [Space]
    [Header("Manager References -----------------------------------------------------------------")]
    [Space]
    [SerializeField] private Witness_Manager Witness_Manager;
    [SerializeField] private Suspicion_Words_Manager Suspicion_Words_Manager;
    [SerializeField] private Murder_Manager Murder_Manager;
    [Space]
    [Header("Text Separators --------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Text_Seperator_0 Text_Seperator_0;
    [Space]
    [Header("Tabs -------------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private CanvasGroup Witness_Tab_CG;
    [SerializeField] private CanvasGroup Suspicion_Words_Tab_CG;
    [SerializeField] private CanvasGroup Sus_Image_EG_CG;

    #endregion

    #region Private Vars
    private bool Initial_Response_Processed = false;
    private bool Is_End_Game = false;
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
        if(Is_End_Game)
            End_Game_Screen();
        if (Is_End_Game && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            StartCoroutine(TypeResponse(Gemini_Api_Handler.Last_Response));
            Is_End_Game = false;
        }
    }

    private IEnumerator TypeResponse(string response)
    {
        while ( Murder_Manager.Suspect_Response_Display.text.Length > 0)
        {
            Murder_Manager.Suspect_Response_Display.text  =  Murder_Manager.Suspect_Response_Display.text .Substring(0,    Murder_Manager.Suspect_Response_Display.text.Length - 1);
            yield return new WaitForSeconds(0.01f);
        }

        foreach (char c in response)
        {
            Murder_Manager.Suspect_Response_Display.text += c;
            yield return new WaitForSeconds(0.01f);
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

    public void On_Ask_Suspect_Button_Pressed()
    {
        if (Murder_Manager != null)
        {
            Murder_Manager.On_Ask_Button_Pressed();
        }
        else
        {
            Debug.LogError("Murder_Manager reference is missing!");
        }
    }

    public void On_Prediction_Button_Pressed()
    {

        if (!Suspicion_Words_Manager.Is_All_Categories_Selected())
        {
            Debug.LogWarning("Lütfen tüm kategorilerden seçim yapın!");
            return;
        }
        Is_End_Game = true;
        Suspicion_Words_Manager.Prepare_Player_Prediction();
        Send_Player_Prediction();
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
        StartCoroutine(Fade_Out_Canvas_Group(Loading_Screen.GetComponent<CanvasGroup>(), 0.2f));
        Text_Seperator_0.Parse_Response(Gemini_Api_Handler.Last_Response);
        Initial_Response_Processed = true;

        // Initialize systems
        Witness_Manager.Initialize_Witness_System();
        Suspicion_Words_Manager.Update_Suspicion_Words_UI();
        Murder_Manager.Initialize_Suspect_System();
    }

    private void Send_Player_Prediction()
    {
        if (!string.IsNullOrEmpty(Suspicion_Words_Manager.Player_Prediction))
        {
            Gemini_Api_Handler.Send_Prompt(Suspicion_Words_Manager.Player_Prediction);
        }
    }

    private void End_Game_Screen()
    {
        StartCoroutine(Fade_Out_Canvas_Group(Witness_Tab_CG, 0.2f));
        StartCoroutine(Fade_Out_Canvas_Group(Suspicion_Words_Tab_CG, 0.1f));

        StartCoroutine(Fade_In_Canvas_Group(Sus_Image_EG_CG, 1.5f));
    }

    public IEnumerator Fade_In_Canvas_Group(CanvasGroup cg, float duration)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    public IEnumerator Fade_Out_Canvas_Group(CanvasGroup cg, float duration)
    {
        float startAlpha = cg.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }

        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}