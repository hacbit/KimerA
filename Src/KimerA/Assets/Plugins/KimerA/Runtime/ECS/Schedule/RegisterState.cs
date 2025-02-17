namespace KimerA.ECS
{
    using System;

    internal sealed class RegisterState<TState> : IRegisterState where TState : Enum
    {
        public TState State;

        public RegisterState(TState state)
        {
            State = state;
        }
    }
}