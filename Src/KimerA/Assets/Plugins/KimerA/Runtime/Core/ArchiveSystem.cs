using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KimerA
{
    /// <summary>
    /// An interface that can be used to mark a class as archivable.
    /// And then it will auto register all the Archive fields in the class.
    /// </summary>
    public interface IArchivable
    {

    }

    internal interface IArchive
    {
        object Value { get; set; }
        Action OnSave { get; set; }
        Action<IArchive> OnLoad { get; set; }
    }

    /// <summary>
    /// A generic archive class that can be used to save and load data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Archive<T> : IArchive
    {
        public T Value;

        object IArchive.Value
        {
            get => Value!;
            set => Value = (T)value;
        }

        public Archive(T data)
        {
            Value = data;
        }

        Action<IArchive> IArchive.OnLoad { get; set; } = data => { };

        Action IArchive.OnSave { get; set; } = () => { };

        public static implicit operator T(Archive<T> archive)
        {
            return archive.Value;
        }

        public static implicit operator Archive<T>(T data)
        {
            return new Archive<T>(data);
        }

        public override bool Equals(object obj)
        {
            return obj is Archive<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value!.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public static class ArchiveSystem
    {
        private static readonly Dictionary<string, IArchive> m_ArchiveInstances = new Dictionary<string, IArchive>();

        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static void RegisterAll<T>(T obj) where T : UnityEngine.Object
        {
            var fields = obj.GetType().GetFields(k_BindingFlags).Where(field => 
                field.FieldType.IsGenericType 
                && field.FieldType.GetGenericTypeDefinition() == typeof(Archive<>)
            );
            foreach (var field in fields)
            {
                Register(field, field.GetValue(obj));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(FieldInfo field, object value)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Archive<>))
            {
                var fieldFullName = field.DeclaringType.FullName + ":" + field.Name;
                if (value is IArchive cell)
                {
                    RegisterInternal(fieldFullName, cell);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RegisterInternal(string fieldFullName, IArchive cell)
        {
            cell.OnSave += () =>
            {
                m_ArchiveInstances[fieldFullName].Value = cell.Value;
            };

            cell.OnLoad += (data) =>
            {
                m_ArchiveInstances[fieldFullName].Value = data.Value;
            };

            m_ArchiveInstances[fieldFullName] = cell;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetArchivePath(string archiveName)
        {
            return FileSystem.GetPath(FileSystem.PathType.AppData, archiveName);
        }

#region Archive Save or Load
        private const string k_ArchiveExtension = ".archive";

        private const string k_ArchiveDirectory = "Archives";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryInitArchive()
        {
            FileSystem.CreateDirectory(k_ArchiveDirectory, FileSystem.PathType.AppData);
        }

        public static void Save(string archiveName)
        {
            TryInitArchive();
            foreach (var archive in m_ArchiveInstances.Values)
            {
                archive.OnSave?.Invoke();
            }
            #if KIMERA_NEWTONSOFTJSON_SUPPORT
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(m_ArchiveInstances);
            #else
            var data = UnityEngine.JsonUtility.ToJson(m_ArchiveInstances);
            #endif
            FileSystem.WriteText(archiveName + k_ArchiveExtension, data, FileSystem.PathType.AppData);
        }

        public static void Load(string archiveName)
        {
            var data = FileSystem.ReadText(archiveName + k_ArchiveExtension, FileSystem.PathType.AppData);
            #if KIMERA_NEWTONSOFTJSON_SUPPORT
            var archives = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, IArchive>>(data);
            #else
            var archives = UnityEngine.JsonUtility.FromJson<Dictionary<string, IArchive>>(data);
            #endif
            if (archives is not null)
            {
                foreach (var archive in archives)
                {
                    m_ArchiveInstances[archive.Key].OnLoad?.Invoke(archive.Value);
                }
            }
        }
#endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear()
        {
            FileSystem.ClearDirectory(k_ArchiveDirectory, FileSystem.PathType.AppData);
        }
    }
}