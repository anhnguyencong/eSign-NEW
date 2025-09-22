namespace ESignature.DAL
{
    public enum JobFileType
    {
        Original = 1,
        Pending = 2,
        Completed = 3
    }

    public enum JobStatus
    {
        Pending = 1,
        Processing = 2, 
        Completed = 3,
        Failed = 4
    }

    public enum CallBackStatus
    {
        Pending = 1, 
        Completed = 2,
        Failed = 3
    }

    public enum JobPriority
    {
        P1 = 1,
        P2 = 2,
        P3 = 3,
        P4 = 4,
        P5 = 5,
        P6 = 6,
        P7 = 7,
        P8 = 8,
        P9 = 9,
        P10 = 10
    }

    public enum JobUpdatePriorityType
    {
        BatchId = 0,
        Job = 1
    }

    public enum VisiblePosition
    {
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4
    }
}