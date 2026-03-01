
//Portable Enums

public enum QuestStatus
{
    Locked=10,
    Available=20,
    InProgress=30,
    Complete=40,
    none=0
}

public enum QuestGiver
{
    NPC=10,
    EventTrigger=20,
    SceneItem=30,
    none=0
}

public enum QuestType
{
    Quest=10,
    Dialog=20,
    none=0
}

public enum QuestObjectiveType
{
    Courier = 10,
    Collect = 20,
    Task = 30,
    Defend=40,
    none = 0
}

public enum QuestTaskType
{
    Talk = 10,
    Pickup = 20,
    Deliver = 30,
    none = 0
}

public enum QuestInventoryStatus
{
    Add=10,
    Remove=20,
    Hidden=30,
    none=0
}

public enum CurrentWorkingNode
{
    QuestStart,
    QuestEnd,
    QuestCollect,
    QuestCourier,
    QuestTask,
    none
};

