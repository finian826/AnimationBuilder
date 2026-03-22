using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "so_NPCQuestList", menuName = "Scriptable Objects/Quest System/NPC/NPC Quest List")]
public class SO_NPCQuestList : ScriptableObject
{
    public string npcID;
    public List<NPCQuestDialogItem> questIDList=new List<NPCQuestDialogItem>();
}
