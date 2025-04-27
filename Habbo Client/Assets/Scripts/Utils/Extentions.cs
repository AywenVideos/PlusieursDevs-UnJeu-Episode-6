using UnityEngine;

public static class Extentions
{
    /// <summary>
 /// Recursively find the first child with the specified tag.
 /// </summary>
 /// <param name="parent">Parent transform.</param>
 /// <param name="tag">Tag to find.</param>
 /// <returns>Transform of the first child found with the tag, or null if none exists.</returns>
    public static Transform FindChildWithTag(this Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            // Check if the current child has the tag
            if (child.tag == tag)
            {
                return child;
            }

            // Recursively search in the child's children
            Transform found = child.FindChildWithTag(tag);
            if (found != null)
            {
                return found;
            }
        }
        // Return null if no child with the tag is found
        return null;
    }
}