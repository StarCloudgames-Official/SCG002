using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarCloudgamesLibrary
{
	public class Singleton<T> : CachedMonoBehaviour where T : CachedMonoBehaviour
	{
		public static T Instance { get; private set; }

		public static T Create(bool dontDestroy = false)
		{
			if (Instance != null) return null;

			var isExist = FindAnyObjectByType<T>();
			if (isExist) return isExist;

			var newSingleton = new GameObject(typeof(T).Name);
			var newComponent = newSingleton.AddComponent<T>();
			if(dontDestroy) DontDestroyOnLoad(newSingleton);
			return newComponent;
		}
		
		protected virtual void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this as T;
		}

		public virtual Awaitable Initialize() => null;
	}
}