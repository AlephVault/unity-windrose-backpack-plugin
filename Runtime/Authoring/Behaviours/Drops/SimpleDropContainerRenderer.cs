using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlephVault.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Drops
            {
                /// <summary>
                ///   <para>
                ///     Simple drop containers are components that render the loot in a single square.
                ///       Loot is a sorted stack of item stacks.
                ///   </para>
                ///   <para>
                ///     This component is not alone: The same game object must have at least one instance
                ///       of <see cref="SpriteRenderer"/> and it MUST be turned into a prefab to be used.
                ///       The use of a <see cref="SortingGroup"/> will help us sorting the sprites
                ///       appropriately.
                ///   </para>
                ///   <para>
                ///     The sprites will render the contents in stacking order: the "last" item in the
                ///       container will be rendered topmost, while the "first" one will be rendered
                ///       last, or hidden in the "bulk": If there are more items to render, than sprites,
                ///       the last sprite will not render a regular item but instead a sprite told to
                ///       be a "generic" image (say, the picture of a dropped purse).
                ///   </para>
                ///   <para>
                ///     This renderer is only useful for simple rendering: stacks consisting on sprite,
                ///       quantity and caption, located inside drop containers at integer positions.
                ///   </para>
                /// </summary>
                [RequireComponent(typeof(SortingGroup))]
                public class SimpleDropContainerRenderer : MonoBehaviour
                {
                    /// <summary>
                    ///   The image to represent when there are more items to be drawn than available
                    ///     sprite rendering slots.
                    /// </summary>
                    [SerializeField]
                    private Sprite backgroundBulkImage;

                    // The renderers to manage.
                    private SpriteRenderer[] renderers;

                    // The stuff being rendered.
                    private SortedDictionary<int, Tuple<Sprite, string, object>> elements;

                    private void Awake()
                    {
                        elements = new SortedDictionary<int, Tuple<Sprite, string, object>>();
                        // Gets all the renderers and assigns them
                        //   a different sorting order
                        renderers = GetComponentsInChildren<SpriteRenderer>();
                        int order = 0;
                        foreach (SpriteRenderer renderer in renderers)
                        {
                            renderer.sortingLayerID = 0;
                            renderer.sortingOrder = order++;
                            renderer.transform.localPosition = Vector3.zero;
                        }
                    }

                    /**
                     * Uncomment this code to debug
                     * PLEASE DON'T DELETE THIS FUNCTION FOREVER.
                     * private void DebugContentToRefresh()
                     * {
                     *     Debug.Log("Contents: " + string.Join(",", (from element in elements select string.Format("{0} -> ({1}: {2})", element.Key, element.Value.Second, element.Value.Third)).ToArray()));
                     * }
                     */

                    private void Refresh()
                    {
                        // Uncomment theses lines to debug.
                        // DebugContentToRefresh();
                        // List<string> debugElements = new List<string>();

                        int currentSize = elements.Count;
                        int renderingSlots = renderers.Length;

                        if (currentSize > renderingSlots)
                        {
                            // Uncomment this line to debug.
                            // debugElements.Add(string.Format("background image"));
                            renderers[0].sprite = backgroundBulkImage;
                            int baseElementIndex = currentSize - renderingSlots;
                            for (int index = 1; index < renderingSlots; index++)
                            {
                                // Uncomment this line to debug.
                                // debugElements.Add(string.Format("{0} -> {1}", index, elements[index + baseElementIndex].Second));
                                renderers[index].sprite = elements[index + baseElementIndex].Item1;
                                renderers[index].enabled = true;
                            }
                        }
                        else if (currentSize == renderingSlots)
                        {
                            for (int index = 0; index < renderingSlots; index++)
                            {
                                // Uncomment this line to debug.
                                // debugElements.Add(string.Format("{0} -> {1}", index, elements[index].Second));
                                renderers[index].sprite = elements[index].Item1;
                                renderers[index].enabled = true;
                            }
                        }
                        else
                        {
                            for (int index = 0; index < currentSize; index++)
                            {
                                // Uncomment this line to debug.
                                // debugElements.Add(string.Format("{0} -> {1}", index, elements[index].Second));
                                renderers[index].sprite = elements[index].Item1;
                                renderers[index].enabled = true;
                            }
                            for (int index = currentSize; index < renderingSlots; index++)
                            {
                                renderers[index].enabled = false;
                            }
                        }

                        // Uncomment this line to debug.
                        // Debug.Log("Rendered contents: " + string.Join(",", debugElements.ToArray()));
                    }

                    /// <summary>
                    ///   Returns an iterator of elements being represented, being an integer position
                    ///     and a tuple of image, caption, and quantity.
                    /// </summary>
                    /// <returns>A new iterator</returns>
                    public IEnumerable<KeyValuePair<int, Tuple<Sprite, string, object>>> Elements()
                    {
                        return elements.AsEnumerable();
                    }

                    /// <summary>
                    ///   Puts an element at certain integer position consisting on image, caption and quantity,
                    ///     and refreshes the contents of the renderer.
                    /// </summary>
                    /// <param name="index">The position to put the element into</param>
                    /// <param name="icon">The element's icon</param>
                    /// <param name="caption">The element's caption</param>
                    /// <param name="quantity">The element's quantity</param>
                    /// <remarks>This function may be used by both add or refresh cases.</remarks>
                    public void RefreshWithPutting(int index, Sprite icon, string caption, object quantity)
                    {
                        elements[index] = new Tuple<Sprite, string, object>(icon, caption, quantity);
                        Refresh();
                    }

                    /// <summary>
                    ///   Removes an element from a certain position, and refreshes the contents of the renderer.
                    /// </summary>
                    /// <param name="index">The index of the element to remove</param>
                    public void RefreshWithRemoving(int index)
                    {
                        if (elements.ContainsKey(index))
                        {
                            elements.Remove(index);
                            Refresh();
                        }
                    }

                    /// <summary>
                    ///   Returns whether this renderer is empty (i.e. has no elements to renderer).
                    /// </summary>
                    /// <returns>Whether it is empty</returns>
                    public bool Empty()
                    {
                        return elements.Count == 0;
                    }
                }
            }
        }
    }
}
