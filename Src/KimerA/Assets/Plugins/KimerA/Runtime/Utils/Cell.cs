using System;
using System.Collections.Generic;

namespace KimerA.Utils
{
    public unsafe struct UnsafeCell<T> where T : unmanaged
    {
        private T m_Value;

        public T Value
        {
            readonly get => m_Value;
            set => m_Value = value;
        }

        public UnsafeCell(T value)
        {
            m_Value = value;
        }

        public unsafe T* AsPointer()
        {
            fixed (T* ptr = &m_Value)
            {
                return ptr;
            }
        }
    }

#region ObservableCell
    /// <summary>
    /// A cell that notifies about the change of the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// The following example demonstrates how to use <see cref="ObservableCell{T}"/>.
    /// <code>
    /// var cell = new ObservableCell&lt;int&gt;(5);
    /// cell.BeforeChange += oldValue => Console.WriteLine($"Before change: {oldValue}");
    /// cell.OnChange += (oldValue, newValue) => Console.WriteLine($"Changed from {oldValue} to {newValue}");
    /// cell.AfterChange += newValue => Console.WriteLine($"After change: {newValue}");
    /// cell.Value = 10;
    /// </code>
    /// </example>
    public class ObservableCell<T> where T : unmanaged
    {
        private T m_Value;

        /// <summary>
        /// If set, some callback will be called.
        /// </summary>
        public T Value
        {
            get => m_Value;
            set
            {
                BeforeChange?.Invoke(m_Value);
                OnChange?.Invoke(m_Value, value);
                m_Value = value;
                AfterChange?.Invoke(value);
            }
        }

        public ObservableCell(T value)
        {
            m_Value = value;
            BeforeChange = oldVal => {};
            AfterChange = newVal => {};
            OnChange = (oldVal, newVal) => {};
        }

        /// <summary>
        /// Process old value before change.
        /// </summary>
        public Action<T> BeforeChange;

        /// <summary>
        /// Process new value after change.
        /// </summary>
        public Action<T> AfterChange;

        /// <summary>
        /// Process old and new values on change.
        /// </summary>
        public Action<T, T> OnChange;

        public static implicit operator T(ObservableCell<T> cell)
        {
            return cell.Value;
        }

        public static implicit operator ObservableCell<T>(T value)
        {
            return new ObservableCell<T>(value);
        }
    }
#endregion

#region LazyCell
    /// <summary>
    /// A cell that contains a value that is created lazily.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyCell<T>
    {
        private T? m_Value;
        private Func<T>? m_Factory;

        public T Value
        {
            get
            {
                if (m_Factory != null)
                {
                    m_Value = m_Factory();
                    m_Factory = null;
                }

                return m_Value!;
            }
        }

        public bool IsValueCreated => m_Factory == null;

        public LazyCell(Func<T> factory)
        {
            m_Value = default;
            m_Factory = factory;
        }
    }
#endregion

#region OnceCell
    /// <summary>
    /// A cell that contains a value that is initialized only once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OnceCell<T>
    {
        private T? m_Value;
        private bool m_IsValueCreated;

        public T GetOrInit(Func<T> factory)
        {
            if (m_IsValueCreated == false)
            {
                m_Value = factory();
                m_IsValueCreated = true;
            }

            return m_Value!;
        }
    }
#endregion

#region OnceLock
    /// <summary>
    /// A cell that contains a value that is initialized only once and is thread-safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OnceLock<T>
    {
        private T? m_Value;
        private volatile bool m_IsValueCreated;
        private readonly object m_Lock = new object();

        public T GetOrInit(Func<T> factory)
        {
            if (m_IsValueCreated == false)
            {
                lock (m_Lock!)
                {
                    if (m_IsValueCreated == false)
                    {
                        m_Value = factory();
                        m_IsValueCreated = true;
                    }
                }
            }

            return m_Value!;
        }

        public bool IsInitialized => m_IsValueCreated;
    }
#endregion

#region HistoryCell
    /// <summary>
    /// A cell that contains a value and its history.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HistoryCell<T> where T : unmanaged
    {
        private T m_Value;
        private readonly List<T> m_History = new();

        public T Value
        {
            get => m_Value;
            set
            {
                m_History.Add(m_Value);
                m_Value = value;
            }
        }

        /// <summary>
        /// The history of the value.
        /// The smaller the index, the older the value.
        /// </summary>
        public IReadOnlyList<T> History => m_History;

        public HistoryCell(T value)
        {
            m_Value = value;
        }

        public static implicit operator T(HistoryCell<T> cell)
        {
            return cell.Value;
        }

        public static implicit operator HistoryCell<T>(T value)
        {
            return new HistoryCell<T>(value);
        }
    }
#endregion

#region PredicateCell
    /// <summary>
    /// A cell that contains a value and a predicate.
    /// The value is set only if the predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>/
    public class PredicateCell<T> where T : unmanaged
    {
        private T m_Value;
        private Func<T, bool>? m_Predicate;

        public T Value
        {
            get => m_Value;
            set
            {
                if (m_Predicate?.Invoke(value) ?? true)
                {
                    m_Value = value;
                }
            }
        }

        public PredicateCell()
        {
            m_Value = default;
            m_Predicate = _ => true;
        }

        public PredicateCell(T value)
        {
            m_Value = value;
            m_Predicate = _ => true;
        }

        public PredicateCell(T value, Func<T, bool> predicate)
        {
            m_Value = value;
            m_Predicate = predicate;
        }

        /// <summary>
        /// Add a predicate.
        /// </summary>
        /// <param name="predicate"></param>
        public void AddPredicate(Func<T, bool> predicate)
        {
            m_Predicate += predicate;
        }

        /// <summary>
        /// Remove an existing predicate.
        /// </summary>
        /// <param name="predicate"></param>
        public void RemovePredicate(Func<T, bool> predicate)
        {
            m_Predicate -= predicate;
        }

        /// <summary>
        /// Clear all predicates.
        /// </summary>
        public void ClearPredicates()
        {
            m_Predicate = _ => true;
        }
    }
#endregion

#region BoundedCell
    /// <summary>
    /// A cell that contains a value and a bound.
    /// The value will be set to the bounded value if the bound is exceeded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoundedCell<T> where T : unmanaged
    {
        private T m_Value;
        private Func<T, (bool, T)>? m_Bound;

        public T Value
        {
            get => m_Value;
            set
            {
                var (isBound, boundedValue) = m_Bound?.Invoke(value) ?? (true, value);
                if (isBound)
                {
                    m_Value = boundedValue;
                }
            }
        }

        public BoundedCell()
        {
            m_Value = default;
            m_Bound = val => (true, val);
        }

        public BoundedCell(T value)
        {
            m_Value = value;
            m_Bound = val => (true, val);
        }

        public BoundedCell(T value, Func<T, (bool, T)> bound)
        {
            m_Value = value;
            m_Bound = bound;
        }

        /// <summary>
        /// Add a bound rule.
        /// </summary>
        /// <param name="bound"></param>
        public void AddBound(Func<T, (bool, T)> bound)
        {
            m_Bound += bound;
        }

        /// <summary>
        /// Remove an existing bound rule.
        /// </summary>
        /// <param name="bound"></param>
        public void RemoveBound(Func<T, (bool, T)> bound)
        {
            m_Bound -= bound;
        }

        /// <summary>
        /// Clear all bound rules.
        /// </summary>
        public void ClearBounds()
        {
            m_Bound = val => (true, val);
        }
    }
#endregion

#region RangeCell
    /// <summary>
    /// A cell that contains a comparable value and a range.
    /// The value will be set to the minimum or maximum value if it is out of range.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeCell<T> where T : unmanaged, IComparable<T>
    {
        private T m_Value;
        private readonly T m_Min;
        private readonly T m_Max;
        
        public T Value
        {
            get => m_Value;
            set
            {
                if (value.CompareTo(m_Min) < 0)
                {
                    m_Value = m_Min;
                }
                else if (value.CompareTo(m_Max) > 0)
                {
                    m_Value = m_Max;
                }
                else
                {
                    m_Value = value;
                }
            }
        }

        public T Min => m_Min;

        public T Max => m_Max;

        public RangeCell(T min, T max)
        {
            m_Min = min;
            m_Max = max;
            Value = default;
        }

        public RangeCell(T min, T max, T value)
        {
            m_Min = min;
            m_Max = max;
            Value = value;
        }
    }
#endregion


}