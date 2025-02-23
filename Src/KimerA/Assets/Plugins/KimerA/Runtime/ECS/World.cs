using System;
using System.Collections.Generic;

namespace KimerA.ECS
{
    public class World
    {
        private Entities entities;
        private Dictionary<List<Type>, int> index;
        private Dictionary<Type, List<Entity>> removedComponents;
        public List<Archetype> Archetypes;
        private long archetypeGeneration;

        public World()
        {
            entities = new Entities();
            Archetypes = new() { new Archetype() };
            index = new() {{new List<Type>(), 0}};
            archetypeGeneration = 0;
            removedComponents = new();
        }
    }
}