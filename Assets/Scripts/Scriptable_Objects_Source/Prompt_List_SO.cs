using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Paragraphs_For_Insp
{
    public string Title;
    [TextArea(3, 10)]
    public string Paragraph;
}

[CreateAssetMenu(fileName = "Prompt_List_SO", menuName = "Scriptable Objects/Prompts/Prompt_List_SO")]
public class Prompt_List_SO : ScriptableObject
{
    //*-----------------------------------------------------------------------------------------//
    [Header("Prompt Listesi ------------------------------------------------------------")]
    [Space]
    public List<Paragraphs_For_Insp> Prompt_List = new List<Paragraphs_For_Insp>();
    //*-----------------------------------------------------------------------------------------//
}
