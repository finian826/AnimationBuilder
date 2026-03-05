using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCGameManager : MonoBehaviour
{
    [SerializeField] private SO_NPCList masterNPCList = null;
    [SerializeField] private List<SO_QuestNode> dialogQuestNodeMaps = new List<SO_QuestNode>();
    private Dictionary<string, List<string>> dialogQuestDictionary = new Dictionary<string, List<string>>(); // dictionary<questNodeID, list<step nodes>>
    private Dictionary<string, QuestStatus> dialogQuestStatusDictionary = new Dictionary<string, QuestStatus>();


    private void Awake()
    {
        //build dictionaries

    }
    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Update()
    {

    }

    private void UpdateQuestStartList()
    {

    }

}
