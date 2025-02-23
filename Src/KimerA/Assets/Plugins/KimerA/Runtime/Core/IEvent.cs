using System;
using System.Collections.Generic;

namespace KimerA
{
    public interface IEvent
    {

    }

    public class EventSystem
    {
        private interface IRegistration { }

        private class Registration<TEvent> : IRegistration where TEvent : IEvent
        {
            public Action<TEvent> Action = _ => { };
        }

        private Dictionary<Type, IRegistration> m_EventMap = new();

        public void Register<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (m_EventMap.TryGetValue(typeof(TEvent), out var registration))
            {
                if (registration is Registration<TEvent> typedRegistration)
                {
                    typedRegistration.Action += action;
                }
            }
            else
            {
                m_EventMap.Add(typeof(TEvent), new Registration<TEvent>{ Action = action });
            }
        }

        public void Unregister<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            if (m_EventMap.TryGetValue(typeof(TEvent), out var registration))
            {
                if (registration is Registration<TEvent> typedRegistration)
                {
                    typedRegistration.Action = typedRegistration.Action - action ?? (_ => { });
                }
            }
        }

        public void Trigger<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (m_EventMap.TryGetValue(typeof(TEvent), out var registration))
            {
                if (registration is Registration<TEvent> typedRegistration)
                {
                    typedRegistration.Action.Invoke(@event);
                }
            }
        }
    }
}