using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KimerA
{
    public interface IState
    {
        List<ITransition> Transitions { get; }
        StateMachine OnInitialize(StateMachine stateMachine);
        void OnEnter() { }
        void OnUpdate() { }
        void OnExit() { }
    }

    public interface ITransition
    {
        Type To { get; }
        Func<bool> Condition => () => true;
        Action OnTransition => () => { };
    }

    public readonly struct Transition<TState> : ITransition where TState : IState
    {
        public Type To { get; }
        public Func<bool> Condition { get; }
        public Action OnTransition { get; }

        public Transition(Func<bool> condition, Action onTransition)
        {
            To = typeof(TState);
            Condition = condition;
            OnTransition = onTransition;
        }
    }

    public class StateMachine
    {
        StateMachine() { }

        public StateMachine(IState defaultState)
        {
            m_States[defaultState.GetType()] = defaultState;
            m_CurrentState = defaultState;
        }

        private IState? m_CurrentState;

        private readonly Dictionary<Type, IState> m_States = new();

        /// <summary>
        /// Set the host object to update the state machine
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public StateMachine SetHost(GameObject @object)
        {
            @object.AddComponent<FsmUpdater>().Register(() =>
            {
                m_CurrentState?.OnUpdate();
                SwitchState();
            });
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StateMachine RegisterState<TState>(TState state) where TState : IState
        {
            m_States[typeof(TState)] = state;
            return this;
        }

        public void SwitchState()
        {
            foreach (var transition in m_CurrentState!.Transitions)
            {
                if (transition.Condition())
                {
                    m_CurrentState.OnExit();
                    transition.OnTransition();
                    m_CurrentState = m_States[transition.To];
                    m_CurrentState.OnEnter();
                    break;
                }
            }
        }
    }

    public sealed class FsmUpdater : MonoBehaviour
    {
        private Action m_UpdateAction = () => { };

        private void Update()
        {
            m_UpdateAction?.Invoke();
        }

        public void Register(Action action)
        {
            m_UpdateAction += action;
        }
    }
}
