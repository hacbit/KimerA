#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;

namespace KimerA.Editor.Utils
{
    [CreateAssetMenu(fileName = "DefaultEditorStorage", menuName = "KimerA/Editor Storage", order = 3)]
    public sealed class EditorStorageSO : ScriptableObject
    {
        internal readonly Dictionary<Type, Dictionary<string, object>> m_StorageDatas = new();

        /// <summary>
        /// 获取存储中指定类型的值，如果不存在则返回类型默认的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueKey"></param>
        /// <returns></returns>
        public T GetValue<T>(string valueKey)
        {
            if (m_StorageDatas.TryGetValue(typeof(T), out var typeData) && typeData.TryGetValue(valueKey, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            var def = default(T);
            SetValue(valueKey, def);
            return def;
        }

        /// <summary>
        /// 获取存储中指定类型的值，如果不存在则返回指定默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string valueKey, T defaultValue)
        {
            if (m_StorageDatas.TryGetValue(typeof(T), out var typeData) && typeData.TryGetValue(valueKey, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            SetValue(valueKey, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 获取存储中指定类型的值，如果不存在则使用提供的函数生成默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueKey"></param>
        /// <param name="defaultValueFunc"></param>
        /// <returns></returns>
        public T GetValueOrElse<T>(string valueKey, Func<T> defaultValueFunc)
        {
            if (m_StorageDatas.TryGetValue(typeof(T), out var typeData) && typeData.TryGetValue(valueKey, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            var defaultValue = defaultValueFunc();
            SetValue(valueKey, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 设置存储中指定类型的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueKey"></param>
        /// <param name="value"></param>
        public void SetValue<T>(string valueKey, T value)
        {
            if (m_StorageDatas.TryGetValue(typeof(T), out var typeData))
            {
                typeData[valueKey] = value;
            }
            else
            {
                typeData = new Dictionary<string, object>
            {
                { valueKey, value },
            };
                m_StorageDatas[typeof(T)] = typeData;
            }
        }
    }
}

#endif