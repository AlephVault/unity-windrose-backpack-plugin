using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Entities
            {
                namespace Objects
                {
                    namespace Bags
                    {
                        using WindRose.Authoring.Behaviours.Entities.Objects;
                        using World.Layers.Drop;
                        using AlephVault.Unity.BackPack.Types.Inventory.Stacks;
                        using AlephVault.Unity.BackPack.Authoring.Behaviours.Inventory.Standard;

                        /// <summary>
                        ///   Bags are single inventories that belong to a map object.
                        ///   They may be backpacks, coffins, boxes, etc. Aside from
                        ///   accessing the underlying single inventory, these bags
                        ///   have features to interact with the underlying map's
                        ///   drop layer.
                        /// </summary>
                        [RequireComponent(typeof(StandardInventory))]
                        [RequireComponent(typeof(MapObject))]
                        public class StandardBag : MonoBehaviour
                        {
                            // The map object this bag belongs to.
                            public MapObject MapObject { get; private set; }

                            // The underlying single inventory.
                            public StandardInventory Inventory { get; private set; }

                            private void Awake()
                            {
                                MapObject = GetComponent<MapObject>();
                                Inventory = GetComponent<StandardInventory>();
                            }

                            /**
                             * These are convenience methods to interact, in particular, with a drop layer.
                             */
                            private DropLayer GetDropLayer()
                            {
                                if (MapObject.ParentMap == null)
                                {
                                    return null;
                                }

                                return MapObject.ParentMap.GetComponentInChildren<DropLayer>();
                            }

                            /// <summary>
                            ///   Drops an element in certain bag's position, into the current object's in-map
                            ///     position.
                            /// </summary>
                            /// <param name="position">The position of the stack to drop</param>
                            /// <param name="quantity">How much to take & drop from the stack</param>
                            /// <returns>Whether it could drop that quantity in the underlying drop layer, or not</returns>
                            /// <remarks>
                            ///   The drop position on which to place the dropped object will be the lower-left
                            ///     corner of the simple bag holder - for this reason this method is better
                            ///     suited for 1x1 objects.
                            /// </remarks>
                            public bool Drop(int position, object quantity = null)
                            {
                                DropLayer dropLayer = GetDropLayer();
                                if (dropLayer == null)
                                {
                                    return false;
                                }

                                Stack found = Inventory.Find(position);
                                if (found != null)
                                {
                                    Stack taken = Inventory.Take(position, quantity, false);
                                    if (taken != null)
                                    {
                                        object finalStackPosition;
                                        // This call will NEVER fail: drop layers have infinite length.
                                        return dropLayer.Push(new Vector2Int((int)MapObject.X, (int)MapObject.Y), taken, out finalStackPosition);
                                    }
                                }

                                return false;
                            }

                            /// <summary>
                            ///   Takes an element from the underlying drop layer, from the position this object
                            ///     is standing at.
                            /// </summary>
                            /// <param name="finalPosition">The returned final position of the just-picked object, if any</param>
                            /// <param name="optimalPick">
                            ///   Whether an optimal pick should be performed. 
                            ///   See <see cref="InventoryManagementStrategyHolder.Put(object, object, Stack, out object, bool?)"/> for more details
                            /// </param>
                            /// <returns>Whether it could pick an item from the floor or not</returns>
                            /// <remarks>
                            ///   The drop position from which objects will be picked will be the lower-left
                            ///     corner of the simple bag holder - for this reason this method is better
                            ///     suited for 1x1 objects.
                            /// </remarks>
                            public bool Pick(out int? finalPosition, bool? optimalPick = null)
                            {
                                DropLayer dropLayer = GetDropLayer();
                                if (dropLayer == null)
                                {
                                    finalPosition = null;
                                    return false;
                                }

                                Vector2Int containerPosition = new Vector2Int((int)MapObject.X, (int)MapObject.Y);
                                Stack found = dropLayer.Last(containerPosition);
                                if (found != null)
                                {
                                    bool result = Inventory.Put(null, found.Clone(), out finalPosition, optimalPick);
                                    if (result)
                                    {
                                        dropLayer.Remove(containerPosition, (int)found.QualifiedPosition.Item1);
                                    }

                                    return result;
                                }

                                finalPosition = null;
                                return false;
                            }
                        }
                    }
                }
            }
        }
    }
}
