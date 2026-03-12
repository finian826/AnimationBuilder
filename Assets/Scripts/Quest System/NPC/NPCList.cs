using System;
using System.Collections.Generic;

[System.Serializable]
public class NPCList 
{
    public string NPCid;
    public string NPCName;
    public QuestGiver startingType = QuestGiver.none;
}
