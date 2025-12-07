using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public class ObjectPooling<T> where T : MonoBehaviour
    {
        private List<T> pools;

        private T registeredPoolPrefab;
        private Transform registeredParent;

        public ObjectPooling(T poolPrefab, int initializeCount, Transform poolParent)
        {
            pools = new List<T>();

            registeredPoolPrefab = poolPrefab;
            registeredParent = poolParent;

            for(int i = 0; i < initializeCount; i++)
            {
                pools.Add(CreatePools());
            }
        }

        private T CreatePools()
        {
            T createdPool = GameObject.Instantiate(registeredPoolPrefab, registeredParent);
            createdPool.gameObject.SetActive(false);
            return createdPool;
        }

        public T GetPool()
        {
            if(pools.Count > 0)
            {
                for(int i = 0; i < pools.Count; i++)
                {
                    if(!pools[i].gameObject.activeSelf)
                    {
                        return pools[i];
                    }
                }
            }

            pools.Add(CreatePools());
            return pools[^1];
        }

        public List<T> GetActivatingPools()
        {
            var activatingPools = new List<T>();
            for(int i = 0; i < pools.Count; i++)
            {
                if(pools[i].gameObject.activeSelf)
                {
                    activatingPools.Add(pools[i]);
                }
            }
            return activatingPools;
        }

        public void DeactiveAllPool()
        {
            GetActivatingPools().ForEach(x => x.gameObject.SetActive(false));
        }
    }
}