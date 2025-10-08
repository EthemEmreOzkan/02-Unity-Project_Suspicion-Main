using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Text_Seperator_0 : MonoBehaviour
{
    //*-----------------------------------------------------------------------------------------*\\
    #region Inspector Tab
    
    [Header("Murder Data ------------------------------------------------------------------------")]
    [Space]
    public List<string> Motives_For_Murder = new List<string>();
    public List<string> Murder_Locations = new List<string>();
    public List<string> Murder_Weapons = new List<string>();
    [Space]
    [Header("Scenarios --------------------------------------------------------------------------")]
    [Space]
    [TextArea(3, 10)]
    public string Murder_Scenario = "";
    //TODO düznle
    #endregion
    //*-----------------------------------------------------------------------------------------*\\
    #region Public Methods

    public void Parse_Response(string Response)
    {
        if (string.IsNullOrEmpty(Response)) return;

        Clear_All_Lists();

        string[] lines = Response.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToArray();

        string Current_Section = "";
        List<string> Current_List = null;
        System.Text.StringBuilder Scenario_Builder = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("#"))
            {
                if (Scenario_Builder != null)
                {
                    Save_Scenario(Current_Section, Scenario_Builder.ToString().Trim());
                    Scenario_Builder = null;
                }

                Current_Section = line;
                Current_List = Get_List_For_Section(Current_Section);

                if (Is_Scenario_Section(Current_Section))
                {
                    Scenario_Builder = new System.Text.StringBuilder();
                }
            }
            else
            {
                if (Scenario_Builder != null)
                {
                    if (Scenario_Builder.Length > 0)
                        Scenario_Builder.AppendLine();
                    Scenario_Builder.Append(line);
                }
                else if (Current_List != null)
                {
                    Current_List.Add(line);
                }
            }
        }

        if (Scenario_Builder != null)
        {
            Save_Scenario(Current_Section, Scenario_Builder.ToString().Trim());
        }
    }

    private void Clear_All_Lists()
    {
        Motives_For_Murder.Clear();
        Murder_Locations.Clear();
        Murder_Weapons.Clear();
        Murder_Scenario = "";
    }

    private List<string> Get_List_For_Section(string Section)
    {
        switch (Section)
        {
            case "#Cinayet_Nedeni":
                return Motives_For_Murder;
            case "#Cinayet_Yeri":
                return Murder_Locations;
            case "#Cinayet_Silahı":
                return Murder_Weapons;
            default:
                return null;
        }
    }

    private bool Is_Scenario_Section(string Section)
    {
        return Section == "#Cinayet_Senaryosu";
    }

    private void Save_Scenario(string Section, string Content)
    {
        if (Section == "#Cinayet_Senaryosu")
            Murder_Scenario = Content;
    }

    [ContextMenu("Debug Print Results")]
    private void Debug_Print_Results()
    {
        Debug.Log($"Cinayet Nedenleri: {string.Join(", ", Motives_For_Murder)}");
        Debug.Log($"Cinayet Yerleri: {string.Join(", ", Murder_Locations)}");
        Debug.Log($"Cinayet Silahları: {string.Join(", ", Murder_Weapons)}");
        Debug.Log($"Cinayet Senaryosu: {Murder_Scenario}");
    }

    #endregion
    //*-----------------------------------------------------------------------------------------*\\
}