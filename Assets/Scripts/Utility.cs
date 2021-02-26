using UnityEngine;
using System.Collections;

public static class Utility
{
	public static T[] AppendArray<T>(T[] array, T elem)
	{
		var ret = new T[array.Length + 1];
		array.CopyTo(ret, 0);
		ret[ret.Length - 1] = elem;
		return ret;
	}

	public delegate void ForEachChildDel(Transform child);

	public static void ForEachChild(Transform t, ForEachChildDel del, bool recursive = true)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (recursive)
				ForEachChild(child, del);
			del(child);
		}
	}

	public static Transform RecursiveFind(this Transform t, string name)
	{
		var toVisit = new Stack();
		toVisit.Push(t);

		while (toVisit.Count > 0)
		{
			var current = toVisit.Pop() as Transform;
			
			if (current.name == name)
				return current;

			for (int i = 0; i < current.childCount; i++)
			{
				toVisit.Push(current.GetChild(i));
			}
		}

		return null;
	}

}
