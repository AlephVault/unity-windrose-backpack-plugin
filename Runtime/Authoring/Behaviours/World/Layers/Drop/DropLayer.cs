using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace World
            {
                namespace Layers
                {
                    namespace Drop
                    {
                        using WindRose.Authoring.Behaviours.World.Layers;
                        using AlephVault.Unity.BackPack.Types.Inventory.Stacks;
                        using AlephVault.Unity.BackPack.Authoring.Behaviours.Inventory;
                        using System.Linq;

                        /// <summary>
                        ///   <para>
                        ///     The drop layer is a multi-container inventory on the floor. It will
                        ///       manage all those inventories and provide convenience methods
                        ///       (which are delegated into the <see cref="InventoryManagementStrategyHolder"/>
                        ///       component) for each position.
                        ///   </para>
                        ///   <para>
                        ///     Container IDs are <see cref="Vector2Int"/> instances.
                        ///   </para>
                        /// </summary>
                        [RequireComponent(typeof(InventoryManagementStrategyHolder))]
                        [RequireComponent(typeof(InventoryDropLayerRenderingManagementStrategy))]
                        public class DropLayer : MapLayer
                        {
                            /**
                             * While you can directly add/remove items using the holder, it is better if you just
                             *   push / pop the items in the floor (we will treat the items as a "stack" of stacks).
                             * 
                             * We could do it in a different way, but perhaps that odd container management would be
                             *   not so efficient in memory.
                             */

                            private InventoryManagementStrategyHolder inventoryHolder;

                            protected override int GetSortingOrder()
                            {
                                return 20;
                            }

                            protected override void Awake()
                            {
                                base.Awake();
                                inventoryHolder = GetComponent<InventoryManagementStrategyHolder>();
                            }

                            /************************************************************
                             * Some convenience methods here.
                             ************************************************************/

                            /// <summary>
                            ///   New convenience method to push an item in certain position.
                            /// </summary>
                            /// <param name="containerPosition">The container ID to push a stack into</param>
                            /// <param name="stack">The stack to push</param>
                            /// <param name="finalStackPosition">The final position of the stack - if not redistributed among the existing stacks</param>
                            /// <returns>Whether the item could be pushed</returns>
                            public bool Push(Vector2Int containerPosition, Stack stack, out object finalStackPosition)
                            {
                                return inventoryHolder.Put(containerPosition, null, stack, out finalStackPosition, true);
                            }

                            /// <summary>
                            ///   Pops a stack from a container position. This means: the last stack will be removed
                            ///     and returned. If the container is empty, then <c>null</c> is returned.
                            /// </summary>
                            /// <param name="containerPosition">The container ID to pop a stack from</param>
                            /// <returns>The popped stack, or null</returns>
                            public Stack Pop(Vector2Int containerPosition)
                            {
                                Stack stack = inventoryHolder.Last(containerPosition);
                                if (stack != null)
                                {
                                    inventoryHolder.Remove(containerPosition, stack.QualifiedPosition.Item1);
                                }
                                return stack;
                            }

                            /************************************************************
                             * Proxy calls to Inventory Holder methods (except for AddListener and related).
                             ************************************************************/

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.StackPairs(object, bool)"/>.
                            /// </summary>
                            public IEnumerable<Tuple<int, Stack>> StackPairs(Vector2Int containerPosition, bool reverse = false)
                            {
                                return from tuple in inventoryHolder.StackPairs(containerPosition, reverse) select new Tuple<int, Stack>((int)tuple.Item1, tuple.Item2);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Find(object, object)"/>.
                            /// </summary>
                            public Stack Find(Vector2Int containerPosition, int stackPosition)
                            {
                                return inventoryHolder.Find(containerPosition, stackPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.FindAll(object, Func{Tuple{object, Stack}, bool}, bool)"/>.
                            /// </summary>
                            public IEnumerable<Stack> FindAll(Vector2Int containerPosition, Func<Tuple<int, Stack>, bool> predicate, bool reverse = false)
                            {
                                return inventoryHolder.FindAll(containerPosition, delegate (Tuple<object, Stack> tuple) { return predicate(new Tuple<int, Stack>((int)tuple.Item1, tuple.Item2)); }, reverse);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.FindAll(object, ScriptableObjects.Inventory.Items.Item, bool)"/>.
                            /// </summary>
                            public IEnumerable<Stack> FindAll(Vector2Int containerPosition, AlephVault.Unity.BackPack.Authoring.ScriptableObjects.Inventory.Items.Item item, bool reverse = false)
                            {
                                return inventoryHolder.FindAll(containerPosition, item, reverse);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.First(object)"/>.
                            /// </summary>
                            public Stack First(Vector2Int containerPosition)
                            {
                                return inventoryHolder.First(containerPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Last(object)"/>.
                            /// </summary>
                            public Stack Last(Vector2Int containerPosition)
                            {
                                return inventoryHolder.Last(containerPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.FindOne(object, Func{Tuple{object, Stack}, bool}, bool)"/>.
                            /// </summary>
                            public Stack FindOne(Vector2Int containerPosition, Func<Tuple<int, Stack>, bool> predicate, bool reverse = false)
                            {
                                return inventoryHolder.FindOne(containerPosition, delegate (Tuple<object, Stack> tuple) { return predicate(new Tuple<int, Stack>((int)tuple.Item1, tuple.Item2)); }, reverse);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.FindOne(object, ScriptableObjects.Inventory.Items.Item, bool)"/>.
                            /// </summary>
                            public Stack FindOne(Vector2Int containerPosition, AlephVault.Unity.BackPack.Authoring.ScriptableObjects.Inventory.Items.Item item, bool reverse = false)
                            {
                                return inventoryHolder.FindOne(containerPosition, item, reverse);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Put(object, object, Stack, out object, bool?)"/>.
                            /// </summary>
                            public bool Put(Vector2Int containerPosition, int stackPosition, Stack stack, int? finalStackPosition, bool? optimalPutOnNullPosition = null)
                            {
                                object finalOStackPosition;
                                bool result = inventoryHolder.Put(containerPosition, stackPosition, stack, out finalOStackPosition, optimalPutOnNullPosition);
                                finalStackPosition = finalOStackPosition == null ? null : (int?)finalOStackPosition;
                                return result;
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Remove(object, object)"/>.
                            /// </summary>
                            public bool Remove(Vector2Int containerPosition, int stackPosition)
                            {
                                return inventoryHolder.Remove(containerPosition, stackPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Merge(object, object, object)"/>
                            /// </summary>
                            public bool Merge(Vector2Int containerPosition, int? destinationStackPosition, int sourceStackPosition)
                            {
                                return inventoryHolder.Merge(containerPosition, destinationStackPosition, sourceStackPosition);
                            }

                            // The other version of `Merge` has little use here.

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Take(object, object, object, bool)"/>.
                            /// </summary>
                            public Stack Take(Vector2Int containerPosition, int stackPosition, object quantity, bool disallowEmpty)
                            {
                                return inventoryHolder.Take(containerPosition, stackPosition, quantity, disallowEmpty);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Split(object, object, object, object, object, out object)"/>. 
                            /// </summary>
                            public bool Split(Vector2Int sourceContainerPosition, int sourceStackPosition, object quantity,
                                              Vector2Int newStackContainerPosition, int newStackPosition, int? finalNewStackPosition)
                            {
                                object finalNewOStackPosition;
                                bool result = inventoryHolder.Split(sourceContainerPosition, sourceStackPosition, quantity,
                                                                    newStackContainerPosition, newStackPosition, out finalNewOStackPosition);
                                finalNewStackPosition = finalNewOStackPosition == null ? null : (int?)finalNewOStackPosition;
                                return result;
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Use(object, object)"/>.
                            /// </summary>
                            public bool Use(Vector2Int containerPosition, int sourceStackPosition)
                            {
                                return inventoryHolder.Use(containerPosition, sourceStackPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Use(object, object, object)"/>.
                            /// </summary>
                            public bool Use(Vector2Int containerPosition, int sourceStackPosition, object argument)
                            {
                                return inventoryHolder.Use(containerPosition, sourceStackPosition, argument);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Clear"/>.
                            /// </summary>
                            public void Clear()
                            {
                                inventoryHolder.Clear();
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Blink"/>.
                            /// </summary>
                            public void Blink()
                            {
                                inventoryHolder.Blink();
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Blink(object)"/>.
                            /// </summary>
                            public void Blink(Vector2Int containerPosition)
                            {
                                inventoryHolder.Blink(containerPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Blink(object, object)"/>.
                            /// </summary>
                            public void Blink(Vector2Int containerPosition, int stackPosition)
                            {
                                inventoryHolder.Blink(containerPosition, stackPosition);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Import(Types.Inventory.SerializedInventory)"/>.
                            /// </summary>
                            public void Import(AlephVault.Unity.BackPack.Types.Inventory.SerializedInventory serializedInventory)
                            {
                                inventoryHolder.Import(serializedInventory);
                            }

                            /// <summary>
                            ///   Convenience method. See <see cref="InventoryManagementStrategyHolder.Export"/>.
                            /// </summary>
                            public AlephVault.Unity.BackPack.Types.Inventory.SerializedInventory Export()
                            {
                                return inventoryHolder.Export();
                            }

                            // Add/Remove listener have little meaning here.
                        }
                    }
                }
            }
        }
    }
}
