using UnityEngine;

static public class MethodExtensionForMonoBehaviourTransform {
	/// <summary>
	/// Gets or adds a component. Usage example:
	/// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T>(this Component child) where T : Component {
		T result = child.GetComponent<T>();
		if (result == null) {
			result = child.gameObject.AddComponent<T>();
		}
		return result;
	}

	/// <summary>
	/// Gets or adds a component using the gameObject. Usage example:
	/// BoxCollider boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T>(this GameObject go) where T : Component {
		T result = go.GetComponent<T>();
		if (result == null) {
			result = go.AddComponent<T>();
		}
		return result;
	}
}