using TMPro;
using UnityEngine;
using System.Collections;

public class Text_Seperator_1 : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("Witness Data ------------------------------------------------------------------------")]
    [Space]
    public string Previous_Witness_Response = "";
    public string Witness_Response = "";
    [Header("User Interface ----------------------------------------------------------------------")]
    [Space]
    [SerializeField] private TextMeshProUGUI Previous_Witness_Response_TMP;
    [SerializeField] private TextMeshProUGUI Witness_Response_TMP;
    [Header("Typewriter Settings -----------------------------------------------------------------")]
    [Space]
    [SerializeField] private float Typewriter_Speed = 0.05f;
    [SerializeField] private string Waiting_Text = "Ekipten yanit bekleniyor";
    [SerializeField] private float Dot_Interval = 0.2f;

    #endregion

    #region Private Vars
    private Coroutine Typewriter_Coroutine;
    private Coroutine Waiting_Dots_Coroutine;
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Public Methods

    public void Parse_Response_Witness(string Response)
    {
        if (string.IsNullOrEmpty(Response))
            return;

        Stop_Waiting_Animation();

        Witness_Response = Response;
        
        if (Typewriter_Coroutine != null)
            StopCoroutine(Typewriter_Coroutine);
        
        Typewriter_Coroutine = StartCoroutine(Typewriter_Effect(Witness_Response, Witness_Response_TMP));
    }

    public void Save_Previous_Response()
    {
        if (!string.IsNullOrEmpty(Witness_Response))
        {
            Previous_Witness_Response = Witness_Response;
            StartCoroutine(Typewriter_Effect(Previous_Witness_Response, Previous_Witness_Response_TMP));
        }
    }

    public void Start_Waiting_Animation()
    {
        if (Waiting_Dots_Coroutine != null)
            StopCoroutine(Waiting_Dots_Coroutine);
        
        Waiting_Dots_Coroutine = StartCoroutine(Waiting_Dots_Effect());
    }

    public void Stop_Waiting_Animation()
    {
        if (Waiting_Dots_Coroutine != null)
        {
            StopCoroutine(Waiting_Dots_Coroutine);
            Waiting_Dots_Coroutine = null;
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Private Methods

    private IEnumerator Typewriter_Effect(string Text, TextMeshProUGUI Target_TMP)
    {
        Target_TMP.text = "";
        
        foreach (char c in Text)
        {
            Target_TMP.text += c;
            yield return new WaitForSeconds(Typewriter_Speed);
        }
        
        Typewriter_Coroutine = null;
    }

    private IEnumerator Waiting_Dots_Effect()
    {
        int Dot_Count = 0;
        
        while (true)
        {
            Dot_Count = (Dot_Count % 3) + 1;
            string Dots = new string('.', Dot_Count);
            Witness_Response_TMP.text = Waiting_Text + Dots;
            
            yield return new WaitForSeconds(Dot_Interval);
        }
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}