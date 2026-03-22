using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestDialogData 
{
    public string questDialogID;
    public CurrentWorkingNode questDialogType;
    public List<string> questDialogStepIDList;
    public Dictionary<string, List<string>> childStepIDDictionary;
    public Dictionary<string, List<string>> parentStepIDDictionary;

    public QuestDialogData()
    {
        questDialogStepIDList = new List<string>();
        childStepIDDictionary = new Dictionary<string, List<string>>();
        parentStepIDDictionary = new Dictionary<string, List<string>>();
        questDialogType = CurrentWorkingNode.none;
    }
}
