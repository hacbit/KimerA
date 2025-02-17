namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RunIfAttribute : Attribute
    {
        public Func<ISchedule, bool> RunIf { get; }

        public RunIfAttribute(Type scheduleType)
        {
            if (scheduleType.IsSubclassOf(typeof(ISchedule)) == false)
            {
                throw new ArgumentException("ScheduleType must be a subclass of ISchedule");
            }

            RunIf = schedule => schedule is ISchedule s && s.GetType() == scheduleType;
        }
    }
}