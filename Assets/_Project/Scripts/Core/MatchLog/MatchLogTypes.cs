namespace DogHeist.Core.MatchLog
{
    /// <summary>Các mốc chỉ ghi lần xảy ra đầu tiên trong ván.</summary>
    public enum MatchMilestone
    {
        FirstBark = 0,
        OwnerWake = 1,
        Spotted = 2,
        LureThrown = 3,
        DogLured = 4,
        Pickup = 5,
    }

    /// <summary>Các bộ đếm số lần trong ván.</summary>
    public enum MatchCounter
    {
        Bark = 0,
        OwnerWake = 1,
        Spotted = 2,
        LureThrown = 3,
        DogDrop = 4,
    }

    /// <summary>Các trạng thái của trộm được cộng dồn thời gian.</summary>
    public enum ThiefStateTime
    {
        InLight = 0,
        Hidden = 1,
        Crouching = 2,
        Sprinting = 3,
    }
}
