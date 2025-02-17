namespace KimerA.ECS
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class App
    {
        public bool IsRunning { get; private set; }

        private readonly Dictionary<Type, IRegisterState> registerStates = new();

        private readonly List<IPlugin> plugins = new();

        private readonly List<ISystem> systems = new();

        private readonly Dictionary<IRegisterState, List<ISystem>> scheduleSystems = new();

        private AppConfig m_AppConfig = new();

        public App()
        {
            InsertState(ScheduleType.PreStartup);
        }

        public delegate void RefAction<T>(ref T arg);

        public App WithConfig(RefAction<AppConfig> config)
        {
            config(ref m_AppConfig);
            return this;
        }

        private void RunSystemsWithState<TState>(TState state) where TState : Enum
        {
            SetState(state);
            var systems = GetSystems<TState>();
            foreach (var system in systems)
            {
                system.Execute();
            }
        }

        /// <summary>
        /// Add a system to the app.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public App AddSystem(ISystem system)
        {
            systems.Add(system);
            return this;
        }

        /// <summary>
        /// Add a single plugin to the app.
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public App AddPlugin(IPlugin plugin)
        {
            plugins.Add(plugin);
            return this;
        }

        /// <summary>
        /// Add some plugins to the app.
        /// </summary>
        /// <param name="plugins"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public App AddPlugins(params IPlugin[] plugins)
        {
            foreach (var plugin in plugins)
            {
                AddPlugin(plugin);
            }

            return this;
        }

        /// <summary>
        /// Insert a state to the app with default value.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public App InsertState<TState>(TState state) where TState : Enum
        {
            registerStates.Add(typeof(TState), new RegisterState<TState>(state));
            return this;
        }

        private void SetState<TState>(TState newStateValue) where TState : Enum
        {
            if (TryGetState<TState>(out var state) && state is not null)
            {
                state.State = newStateValue;
            }
        }

        private bool TryGetState<TState>(out RegisterState<TState>? state) where TState : Enum
        {
            if (registerStates.TryGetValue(typeof(TState), out var registerState))
            {
                state = (RegisterState<TState>)registerState;
                return true;
            }
            state = default;
            return false;
        }

        private IEnumerable<ISystem> GetSystems<TState>() where TState : Enum
        {
            if (TryGetState<TState>(out var state))
            {
                if (state is not null && scheduleSystems.TryGetValue(state, out var systems))
                {
                    foreach (var system in systems)
                    {
                        if (system is ISystem tSystem)
                        {
                            yield return tSystem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load all plugins and systems and run the app.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Run()
        {
            foreach (var plugin in plugins)
            {
                plugin.OnPluginLoad(this);
            }

            IsRunning = true;

            RunSystemsWithState(ScheduleType.PreStartup);
            RunSystemsWithState(ScheduleType.Startup);
            RunSystemsWithState(ScheduleType.PostStartup);

            Task.Run(() => RunAsync(m_AppConfig.FrameRate));
        }

        private async Task RunAsync(int frameRate = 60)
        {
            while (true)
            {
                if (IsRunning == false) return;

                RunSystemsWithState(ScheduleType.First);
                RunSystemsWithState(ScheduleType.PreUpdate);
                RunSystemsWithState(ScheduleType.StateTransition);
                RunSystemsWithState(ScheduleType.RunFixedMainLoop);
                RunSystemsWithState(ScheduleType.Update);
                RunSystemsWithState(ScheduleType.PostUpdate);
                RunSystemsWithState(ScheduleType.Last);

                await Task.Delay(1000 / frameRate);
            }
        }
    }
}