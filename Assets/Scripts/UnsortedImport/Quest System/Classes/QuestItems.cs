using System;
using System.Collections.Generic;

[Serializable]
public class QuestItems
{
    public int questItem;
    public int questItemQuantity;
    public QuestInventoryStatus questIntialItemInventory = QuestInventoryStatus.none;
    public string targetNPC;
    public string trackerStepDecscription;
    public QuestInventoryStatus targetInventoryStatus = QuestInventoryStatus.none;
    public bool stepCompleted = false;//used in main quest routines
}
