using UnityEngine;

public class Text_Seperator_1 : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab

    [Header("Witness Data ------------------------------------------------------------------------")]
    [Space]
    public string Previous_Witness_Response = "";
    public string Witness_Response = "";

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Public Methods

    public void Parse_Response_Witness(string Response)
    {
        if (string.IsNullOrEmpty(Response))
            return;

        Witness_Response = Response;
    }

    public void Save_Previous_Response()
    {
        if (!string.IsNullOrEmpty(Witness_Response))
        {
            Previous_Witness_Response = Witness_Response;
        }
    }

    public void Clear_Responses()
    {
        Previous_Witness_Response = "";
        Witness_Response = "";
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}