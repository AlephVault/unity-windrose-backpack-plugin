using System;
using UnityEngine;

namespace GameMeanMachine.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            using Drops;
            using GameMeanMachine.Unity.BackPack.Authoring.ScriptableObjects.Inventory.Items;
            using GameMeanMachine.Unity.BackPack.Authoring.ScriptableObjects.Inventory.Items.RenderingStrategies;
            using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;

            namespace World
            {
                namespace Layers
                {
                    namespace Drop
                    {
                        /// <summary>
                        ///   This listener renders a matrix of M x N containers, since it will be related to a map's
                        ///     <see cref="DropLayer"/>. It will do this by creating/refreshing/destroying a lot of
                        ///     <see cref="SimpleDropContainerRenderer"/> instances (one on each map's position).
                        /// </summary>
                        [RequireComponent(typeof(InventoryMapSizedPositioningManagementStrategy))]
                        public class InventoryDropLayerRenderingListener : MonoBehaviour, InventoryDropLayerRenderingManagementStrategy.RenderingListener
                        {
                            private SimpleDropContainerRenderer[,] dropContainers;
                            // We are completely sure we have a PositioningStrategy in the underlying object
                            private InventoryMapSizedPositioningManagementStrategy positioningStrategy;

                            /// <summary>
                            ///   A prefab that MUST be set. It will be used to spawn the renderers for the
                            ///     drop containers.
                            /// </summary>
                            [SerializeField]
                            private SimpleDropContainerRenderer containerPrefab;

                            protected void Awake()
                            {
                                positioningStrategy = GetComponent<InventoryMapSizedPositioningManagementStrategy>();
                                try
                                {
                                    Map map = GetComponent<DropLayer>().Map;
                                    dropContainers = new SimpleDropContainerRenderer[map.Width, map.Height];
                                }
                                catch (NullReferenceException)
                                {
                                    Debug.LogError("This rendering listener must be bound to an object being also a DropLayer");
                                    Destroy(gameObject);
                                }
                            }

                            private SimpleDropContainerRenderer getContainerFor(Vector2Int position, bool createIfMissing = true)
                            {
                                // Retrieves, or clones (if createIsMissing), a container for a specific (x, y) position
                                SimpleDropContainerRenderer container = dropContainers[position.x, position.y];
                                if (container == null && createIfMissing)
                                {
                                    container = Instantiate(containerPrefab.gameObject, transform).GetComponent<SimpleDropContainerRenderer>();
                                    dropContainers[position.x, position.y] = container;
                                    container.transform.localPosition = new Vector3(position.x, position.y);
                                }
                                return container;
                            }

                            private void destroyContainerFor(Vector2Int position)
                            {
                                // Destroys a container, if existing, at an (x, y) position
                                SimpleDropContainerRenderer container = dropContainers[position.x, position.y];
                                if (container != null)
                                {
                                    Destroy(container.gameObject);
                                    dropContainers[position.x, position.y] = null;
                                }
                            }

                            /// <summary>
                            ///   <para>
                            ///     Refreshing the stack involves maybe-instantiating a container renderer, and then
                            ///       refresh-putting an item there.
                            ///   </para>
                            ///   <para>
                            ///     See <see cref="InventoryRenderingManagementStrategy.StackWasUpdated(object, object, Types.Inventory.Stacks.Stack)"/>
                            ///       for more information on the method's signature.
                            ///   </para>
                            /// </summary>
                            /// <param name="containerPosition">The (x, y) in-map position</param>
                            /// <param name="stackPosition">The in-place index</param>
                            /// <param name="item">The item to render</param>
                            /// <param name="quantity">The quantity to render</param>
                            public void UpdateStack(Vector2Int containerPosition, int stackPosition, Item item, object quantity)
                            {
                                // Adds a stack to a container (creates the container if absent).
                                SimpleDropContainerRenderer container = getContainerFor(containerPosition, true);
                                ItemIconTextRenderingStrategy strategy = item.GetRenderingStrategy<ItemIconTextRenderingStrategy>();
                                container.RefreshWithPutting(stackPosition, strategy.Icon, strategy.Caption, quantity);
                            }

                            /// <summary>
                            ///   <para>
                            ///     Removing a stack involves removing the item from the container, and then maybe-destroying
                            ///       the container if empty.
                            ///   </para>
                            ///   <para>
                            ///     See <see cref="InventoryRenderingManagementStrategy.StackWasRemoved(object, object)"/>
                            ///       for more information on the method's signature.
                            ///   </para>
                            /// </summary>
                            /// <param name="containerPosition">The (x, y) in-map position</param>
                            /// <param name="stackPosition">The in-place index</param>
                            public void RemoveStack(Vector2Int containerPosition, int stackPosition)
                            {
                                // Removes a stack from a container (if the container exists).
                                // Removes the container if empty.
                                SimpleDropContainerRenderer container = getContainerFor(containerPosition, false);
                                if (container != null)
                                {
                                    container.RefreshWithRemoving(stackPosition);
                                    if (container.Empty())
                                    {
                                        Destroy(container.gameObject);
                                        dropContainers[containerPosition.x, containerPosition.y] = null;
                                    }
                                }
                            }

                            /// <summary>
                            ///   Clearing all the containers involves destroying the renderer objects.
                            /// </summary>
                            public void Clear()
                            {
                                // Destroys all the containers.
                                foreach (object position in positioningStrategy.Positions())
                                {
                                    destroyContainerFor((Vector2Int)position);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
