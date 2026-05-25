using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A class defining extensions for <tt>Transform</tt>s.
/// </summary>
public static class TransformExtensions
{
    /// <param name="expectedCount">
    ///     How many children are expected to have the given tag.
    /// </param>
    /// <returns>
    ///     All children of the current transform with a tag matching the given one.
    /// </returns>
    public static Transform[] GetChildrenWithTag(this Transform parentTransform, string tag, int expectedCount = 1)
    {
        List<Transform> transforms = new(expectedCount);

        foreach (Transform transform in parentTransform)
        {
            if (transform.gameObject.CompareTag(tag)) transforms.Add(transform);
        }

        return transforms.ToArray();
    }

    /// <returns>
    ///     The transform's parent's parent.
    /// </returns>
    public static Transform Grandparent(this Transform transform)
    {
        return transform.parent.parent;
    }

    /// <returns>
    ///     The given component in the first parent/aunt/uncle of this transform that has it.
    /// </returns>
    public static T GetComponentInParents<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.Grandparent().GetComponentInChildren<T>(includeInactive);
    }

    /// <returns>
    ///     The given component in the first sibling of this transform (including itself) that has it.
    /// </returns>
    public static T GetComponentInSiblings<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.parent.GetComponentInChildren<T>(includeInactive);
    }

    /// <returns>
    ///     All of the given components in the parents/piblings of this transform.
    /// </returns>
    public static T[] GetComponentsInParents<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.Grandparent().GetComponentsInChildren<T>(includeInactive);
    }

    /// <returns>
    ///     All of the given components in the siblings of this transform (including itself).
    /// </returns>
    public static T[] GetComponentsInSiblings<T>(this Transform transform, bool includeInactive = false)
    {
        return transform.parent.GetComponentsInChildren<T>(includeInactive);
    }
}