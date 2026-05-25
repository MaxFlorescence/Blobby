using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static Transform[] GetChildrenWithTag(this Transform parentTransform, string tag, int expectedCount = 1)
    {
        List<Transform> transforms = new(expectedCount);

        foreach (Transform transform in parentTransform)
        {
            if (transform.gameObject.CompareTag(tag)) transforms.Add(transform);
        }

        return transforms.ToArray();
    }

    public static Transform Grandparent(this Transform transform)
    {
        return transform.parent.parent;
    }

    public static T GetComponentInParents<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.Grandparent().GetComponentInChildren<T>(includeInactive);
    }

    public static T GetComponentInSiblings<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.parent.GetComponentInChildren<T>(includeInactive);
    }

    public static T[] GetComponentsInParents<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.Grandparent().GetComponentsInChildren<T>(includeInactive);
    }

    public static T[] GetComponentsInSiblings<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.parent.GetComponentsInChildren<T>(includeInactive);
    }
}