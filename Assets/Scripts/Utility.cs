using UnityEngine;

public class Utility
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
}
