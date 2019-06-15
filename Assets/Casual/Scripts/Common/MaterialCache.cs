using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Casual
{
	public class MaterialCache
	{
		public ulong hash { get; private set; }
        public bool unique { get; private set; }
		public int referenceCount { get; private set; }
		public Texture texture { get; private set; }
		public Material material { get; private set; }

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void ClearCache()
		{
			foreach (var cache in materialCaches.Values)
			{
				cache.material = null;
			}
			materialCaches.Clear();

            foreach(var pool in uniqueMaterialCaches.Values)
            {
                while(pool.Count > 0)
                {
                    var cache = pool.Dequeue();
                    cache.material = null;
                }
            }
            uniqueMaterialCaches.Clear();
		}
#endif
        public static Dictionary<ulong/*hash*/, MaterialCache> materialCaches = new Dictionary<ulong, MaterialCache>();
        public static Dictionary<ulong/*hash*/, Queue<MaterialCache>> uniqueMaterialCaches = new Dictionary<ulong, Queue<MaterialCache>>();

        private static MaterialCache GetCache(ulong hash, bool unique)
        {
            MaterialCache cache = null;
            if (unique)
            {
                if (uniqueMaterialCaches.ContainsKey(hash))
                {
                    while (cache == null && uniqueMaterialCaches[hash].Count > 0)
                    {
                        cache = uniqueMaterialCaches[hash].Dequeue();
                        cache = cache.material == null ? null : cache;
                    }
                }
            }
            else if (materialCaches.ContainsKey(hash) && materialCaches[hash].material != null)
            {
                cache = materialCaches[hash];
                cache.referenceCount++;
            }
            return cache;
        }

        public static MaterialCache Register(ulong hash, Texture texture, System.Func<Material> onCreateMaterial, bool unique = false)
		{
            MaterialCache cache = GetCache(hash, unique);

            if(cache == null)
            {
                cache = new MaterialCache()
                {
                    hash = hash,
                    material = onCreateMaterial(),
                    referenceCount = 1,
                    texture = texture,
                    unique = unique,
                };

                if(!unique)
                {
                    materialCaches[hash] = cache;
                }
            }

            return cache;
		}

		public static MaterialCache Register(ulong hash, System.Func<Material> onCreateMaterial, bool unique = false)
		{
            MaterialCache cache = GetCache(hash, unique);

            if (cache == null)
            {
                cache = new MaterialCache()
                {
                    hash = hash,
                    material = onCreateMaterial(),
                    referenceCount = 1,
                    unique = unique,
                };

                if (!unique)
                {
                    materialCaches[hash] = cache;
                }
            }

            return cache;
        }

		public static void Unregister(MaterialCache cache)
		{
			if (cache == null)
				return;

            if(cache.unique)
            {
                if(!uniqueMaterialCaches.ContainsKey(cache.hash))
                {
                    uniqueMaterialCaches[cache.hash] = new Queue<MaterialCache>();
                }
                uniqueMaterialCaches[cache.hash].Enqueue(cache);
            }
            else
            {
                cache.referenceCount--;
                if (cache.referenceCount <= 0)
                {
                    materialCaches.Remove(cache.hash);
                    cache.material = null;
                }
            }
		}
	}
}