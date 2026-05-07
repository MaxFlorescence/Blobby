using System;
using UnityEngine.UI;

/// <summary>
///     A class defining extensions for <tt>Selectable</tt>s.
/// </summary>
public static class SelectableExtensions
{
    /// <summary>
    ///     Returns the value of the given selectable. Implemented for:
    ///     <code>
    ///         Slider (T == float)
    ///         Toggle (T == bool)
    ///         Dropdown (T == int)
    ///     </code>
    /// </summary>
    public static T GetValue<T>(this Selectable selectable)
    {
        Type itemType = typeof(T);

        return selectable switch {
            Slider slider when (
                selectable is Slider &&
                itemType == typeof(float)
            ) => (T)Convert.ChangeType(slider.value, itemType),

            Toggle toggle when (
                selectable is Toggle &&
                itemType == typeof(bool)
            ) => (T)Convert.ChangeType(toggle.isOn, itemType),

            Dropdown dropdown when (
                selectable is Dropdown &&
                itemType == typeof(int)
            ) => (T)Convert.ChangeType(dropdown.value, itemType),

            _ => throw new InvalidCastException()
        };
    }
}