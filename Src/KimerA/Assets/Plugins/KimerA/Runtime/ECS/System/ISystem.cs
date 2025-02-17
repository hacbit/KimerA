namespace KimerA.ECS
{
    public interface ISystem : ISchedule
    {
        SystemParams Params { get; set; }
        void Execute();
    }
}