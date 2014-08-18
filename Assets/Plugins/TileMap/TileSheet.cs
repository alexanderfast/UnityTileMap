using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTileMap
{
    [Serializable]
    public class TileSheet : ScriptableObject
    {
        [SerializeField]
        private List<TileEntry> m_tiles = new List<TileEntry>();

        public IEnumerable<string> Names
        {
            get { return m_tiles.Select(x => x.Sprite.name).ToList(); }
        }

        public IEnumerable<int> Ids
        {
            get { return m_tiles.Select(x => x.Id).ToList(); }
        }

        public int Count
        {
            get { return m_tiles.Count; }
        }

        public Sprite Get(int id)
        {
            TileEntry entry = GetEntry(id);
            if (entry == null)
                throw new KeyNotFoundException(id.ToString());
            return entry.Sprite;
        }

        public int Lookup(string spriteName)
        {
            TileEntry entry = GetEntry(spriteName);
            if (entry == null)
                throw new KeyNotFoundException(spriteName);
            return entry.Id;
        }

        public string Lookup(int id)
        {
            return m_tiles.FirstOrDefault(x => x.Id == id).Sprite.name;
        }

        public bool Contains(string name)
        {
            return GetEntry(name) != null;
        }

        public bool Contains(int id)
        {
            return GetEntry(id) != null;
        }

        public int Add(Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException("sprite");
            var entry = new TileEntry
            {
                Id = GenerateNewId(),
                Sprite = sprite
            };
            m_tiles.Add(entry);
            return entry.Id;
        }

        public void Remove(int id)
        {
            for (int i = 0; i < m_tiles.Count; i++)
            {
                if (m_tiles[i].Id == id)
                {
                    m_tiles.RemoveAt(i);
                    return;
                }
            }
        }

        private TileEntry GetEntry(int id)
        {
            return m_tiles.FirstOrDefault(entry => entry.Id == id);
        }

        private TileEntry GetEntry(string spriteName)
        {
            return m_tiles.FirstOrDefault(entry => entry.Sprite.name == spriteName);
        }

        // returns the lowest integer value currently not used by an entry
        private int GenerateNewId()
        {
            for (int id = 0;; id++)
            {
                bool valid = true;
                foreach (TileEntry entry in m_tiles)
                {
                    if (entry.Id == id)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                    return id;
            }
        }

        [Serializable]
        private class TileEntry
        {
            public int Id;
            public Sprite Sprite;
        }
    }
}
