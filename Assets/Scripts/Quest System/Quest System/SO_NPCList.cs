using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "so_NPCList", menuName = "Scriptable Objects/Quests/NPC List")]
public class SO_NPCList : ScriptableObject
{
    [SerializeField] public List<NPCList> list;
}
