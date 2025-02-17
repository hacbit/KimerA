namespace KimerA.ECS
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class SystemFuncAttribute : Attribute
    {
        public ISchedule Schedule;

        public SystemFuncAttribute(ScheduleType scheduleType)
        {
            Schedule = scheduleType.ToSchedule();
        }
    }
}