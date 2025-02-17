namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RunAfterAttribute : Attribute
    {
        public Func<ISchedule, bool> RunAfter { get; }

        public RunAfterAttribute(Type scheduleType)
        {
            if (scheduleType.IsSubclassOf(typeof(ISchedule)) == false)
            {
                throw new ArgumentException("ScheduleType must be a subclass of ISchedule");
            }

            RunAfter = schedule => schedule is ISchedule s && s.GetType() == scheduleType;
        }
    }
}