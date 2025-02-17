namespace KimerA.ECS
{
    using System.Collections.Generic;

    public class ScheduleSystem
    {
        private readonly ISystem system;

        /// <summary>
        /// Base schedule
        /// </summary>
        private readonly ScheduleType baseSchedule;

        private readonly List<ISchedule> schedules = new();

        public ScheduleSystem(ISystem system, ScheduleType baseSchedule)
        {
            this.system = system;
            this.baseSchedule = baseSchedule;
        }
    }
}