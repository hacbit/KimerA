using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KimerA.ECS
{
    public readonly struct Entity : IEntity, IQueryable
    {
        private readonly int id;

        public Entity()
        {
            // get random int
            id = new Random().Next();
        }

        public Entity(int id)
        {
            this.id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Id() => id;
    }

    internal struct Entities
    {
        public Dictionary<Entity, Location> EntityLocations;

        public bool Free(Entity entity, out Location location)
        {
            return EntityLocations.Remove(entity, out location);
        }

        public bool Contains(Entity entity)
        {
            return EntityLocations.ContainsKey(entity);
        }

        public void Clear()
        {
            EntityLocations.Clear();
        }

        public ref Location GetRef(Entity entity)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(EntityLocations, entity);
        }

        public void Insert(Entity entity, Location location)
        {
            EntityLocations.Add(entity, location);
        }

        public bool Get(Entity entity, out Location location)
        {
            return EntityLocations.TryGetValue(entity, out location);
        }
    }

    public struct Location
    {
        public int archetype;
        public int index;
    }

    public class EntityBuilder
    {
        private StrongBox<byte?> storage;
        private long cursor;
        
    }
}