namespace KimerA.ECS
{
    public enum ScheduleType
    {
        // On the first run of the schedule (and only on the first run)
        PreStartup,
        Startup,
        PostStartup,

        // Then it will run the following in a loop
        First,
        PreUpdate,
        StateTransition,
        RunFixedMainLoop,
        Update,
        PostUpdate,
        Last,
    }

    public static class ScheduleTypeExtensions
    {
        public static ISchedule ToSchedule(this ScheduleType scheduleType)
        {
            return scheduleType switch
            {
                ScheduleType.PreStartup => new PreStartup(),
                ScheduleType.Startup => new Startup(),
                ScheduleType.PostStartup => new PostStartup(),
                ScheduleType.First => new First(),
                ScheduleType.PreUpdate => new PreUpdate(),
                ScheduleType.StateTransition => new StateTransition(),
                ScheduleType.RunFixedMainLoop => new RunFixedMainLoop(),
                ScheduleType.Update => new Update(),
                ScheduleType.PostUpdate => new PostUpdate(),
                ScheduleType.Last => new Last(),
                _ => new Update(),
            };
        }
    }

    public struct PreStartup : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.PreStartup;
    }

    public struct Startup : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.Startup;
    }

    public struct PostStartup : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.PostStartup;
    }

    public struct First : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.First;
    }

    public struct PreUpdate : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.PreUpdate;
    }

    public struct StateTransition : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.StateTransition;
    }

    public struct RunFixedMainLoop : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.RunFixedMainLoop;
    }

    public struct Update : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.Update;
    }

    public struct PostUpdate : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.PostUpdate;
    }

    public struct Last : ISchedule
    {
        public readonly ScheduleType BaseSchedule => ScheduleType.Last;
    }
}