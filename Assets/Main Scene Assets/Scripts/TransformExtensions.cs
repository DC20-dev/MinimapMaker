using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static Transform[] GetAllChildren(this Transform gameObject)
	{
		Transform[] children = new Transform[gameObject.childCount];

		for (int i = 0; i < gameObject.childCount; i++)
		{
			children[i] = gameObject.GetChild(i);
		}
		return children;
	}
}
