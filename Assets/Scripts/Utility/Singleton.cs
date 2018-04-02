using UnityEngine;

public class Singleton<T> : Tracer where T : Tracer {
	private static T _instance;

	private static object _lock = new object();
	
	public static T Instance {
		get {
			lock (_lock) {
				if (_instance == null) {
					_instance = (T) FindObjectOfType(typeof(T));

					if (FindObjectsOfType(typeof(T)).Length > 1) {
						Debug.LogError("[Singleton] Multiple instances of type " + typeof(T));
						return _instance;
					}

					if (_instance == null) {
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "[Singleton] " + typeof(T).ToString();
                        
						Debug.Log("[Singleton] Creating A New  " + typeof(T));
					}
				}

				return _instance;
			}
		}
	}
}
