using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_Menu_Manager : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("LLM ------ -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private Api_Key_SO Api_Key_SO;
    [SerializeField] private Gemini_Api_Handler Gemini_Api_Handler;
    [Space]
    [Header("References -------------------------------------------------------------------------")]
    [Space]
    [SerializeField] private CanvasGroup Menu_Components_CG;
    [SerializeField] private TMP_InputField Player_InputField;

    private bool Test_Key_Button_Clicked = false;

    #endregion
    //*-----------------------------------------------------------------------------------------*\\

    void Update()
    {
        if (Test_Key_Button_Clicked && Gemini_Api_Handler.Is_Request_In_Progress && !Gemini_Api_Handler.Is_Response_Received)
        {
            Player_InputField.text = "Keyiniz test ediliyor lutfen bekleyiniz... \n Internet hızınıza baglı olarak bu süre artabilir...";
        }
        else if(Test_Key_Button_Clicked && !Gemini_Api_Handler.Is_Request_In_Progress && Gemini_Api_Handler.Is_Response_Received)
        {
            Player_InputField.text = Gemini_Api_Handler.Last_Response;
            Test_Key_Button_Clicked = false;
        }
    }

    //*-----------------------------------------------------------------------------------------*\\

    #region Public Methods

    public void Play_Game()
    {

        StartCoroutine(Fade_Out_Canvas_Group(Menu_Components_CG, 0.5f));
    }
    public void Quit_Game()
    {
        Application.Quit();
    }
    public void Test_Api_Key()
    {
        Api_Key_SO.Player_Key = Player_InputField.text;
        Gemini_Api_Handler.Api_Key = Api_Key_SO.Player_Key;
        Gemini_Api_Handler.Send_Prompt("Api keyinin çalışıp çalışmadığı test edilmekte eğer key çalışmaktaysa yanıt olarak sadece aşağıdaki cümleyi yaz \n 'KEY CALISMAKTA \n IYI EGLENCELER'");
        Test_Key_Button_Clicked = true;
    }

    //*-----------------------------------------------------------------------------------------*\\

    private IEnumerator Fade_In_Canvas_Group(CanvasGroup cg, float duration)
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

    private IEnumerator Fade_Out_Canvas_Group(CanvasGroup cg, float duration)
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
        SceneManager.LoadScene(1);
    }
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}
