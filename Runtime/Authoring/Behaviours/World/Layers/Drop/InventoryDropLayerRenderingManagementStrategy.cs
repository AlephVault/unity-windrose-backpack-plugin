using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameMeanMachine.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            using Drops;
            using AlephVault.Unity.Support.Generic.Vendor.IUnified.Authoring.Types;
            using AlephVault.Unity.Support.Generic.Vendor.IUnified.Authoring.Types.IUnifiedContainerBase;
            using GameMeanMachine.Unity.BackPack.Authoring.ScriptableObjects.Inventory.Items;
            using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory.ManagementStrategies.SpatialStrategies;
            using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory.ManagementStrategies.RenderingStrategies;

            namespace World
            {
                namespace Layers
                {
                    namespace Drop
                    {
                        /// <summary>
                        ///   This strategy renders a matrix of M x N containers, since it will be related to a map's
                        ///     <see cref="DropLayer"/>. It will do this by creating/refreshing/destroying a lot of
                        ///     <see cref="SimpleDropContainerRenderer"/> instances (one on each map's position).
                        /// </summary>
                        [RequireComponent(typeof(InventoryMapSizedPositioningManagementStrategy))]
                        [RequireComponent(typeof(InventoryInfinite1DIndexedSpatialManagementStrategy))]
                        [RequireComponent(typeof(RenderingListener))]
                        public class InventoryDropLayerRenderingManagementStrategy : Inventory1DIndexedStaticRenderingManagementStrategy
                        {
                            /// <summary>
                            ///   Listeners will refresh the stack position at a certain index in a certain (x, y) position.
                            ///     Only one listener will be accounted for, so no kind of "connection" will occur: the listener
                            ///     will exist in the same object of the renderer.
                            /// </summary>
                            public interface RenderingListener
                            {
                                void UpdateStack(Vector2Int position, int stackPosition, Item item, object quantity);
                                void RemoveStack(Vector2Int position, int stackPosition);
                                void Clear();
                            }

                            /// <summary>
                            ///   Just a container for <see cref="RenderingListener"/> interfaces. 
                            /// </summary>
                            [Serializable]
                            public class RenderingListenerContainer : IUnifiedContainer<RenderingListener> { }

                            /// <summary>
                            ///   The listener chosen as the main one. It is the one that will receive
                            ///     updates from the inventory. The listener must be a component in the
                            ///     same object.
                            /// </summary>
                            [SerializeField]
                            private RenderingListenerContainer mainListener;

                            protected override void Awake()
                            {
                                base.Awake();
                                if (mainListener.Result == null)
                                {
                                    Debug.LogWarning("Picking the first available listener!");
                                    mainListener.Result = GetComponent<RenderingListener>();
                                }
                                // The component WILL be a MonoBehaviour, and with the same gameObject.
                            }

                            /// <summary>
                            ///   <para>
                            ///     Notifies the listener to set/update a stack's data.
                            ///   </para>
                            ///   <para>
                            ///     See <see cref="InventoryRenderingManagementStrategy.StackWasUpdated(object, object, Types.Inventory.Stacks.Stack)"/>
                            ///       for more information on the method's signature.
                            ///   </para>
                            /// </summary>
                            /// <param name="containerPosition">The (x, y) in-map position to clear</param>
                            /// <param name="stackPosition">The in-container position to clear</param>
                            /// <param name="item">The item to render</param>
                            /// <param name="quantity">The quantity to render</param>
                            protected override void StackWasUpdated(object containerPosition, int stackPosition, Item item, object quantity)
                            {
                                // Adds a stack to a container (creates the container if absent).
                                mainListener.Result.UpdateStack((Vector2Int)containerPosition, stackPosition, item, quantity);
                            }

                            /// <summary>
                            ///   <para>
                            ///     Notifies the listener to remove a stack's data.
                            ///   </para>
                            ///   <para>
                            ///     See <see cref="InventoryRenderingManagementStrategy.StackWasRemoved(object, object)"/>
                            ///       for more information on the method's signature.
                            ///   </para>
                            /// </summary>
                            /// <param name="containerPosition">The (x, y) in-map position to clear</param>
                            /// <param name="stackPosition">The in-container position to clear</param>
                            protected override void StackWasRemoved(object containerPosition, int stackPosition)
                            {
                                // Tells the listener to remove a stack from a container (if the container exists).
                                mainListener.Result.RemoveStack((Vector2Int)containerPosition, stackPosition);
                            }

                            /// <summary>
                            ///   Notifies the listener to clear its content.
                            /// </summary>
                            public override void EverythingWasCleared()
                            {
                                mainListener.Result.Clear();
                            }
                        }

#if UNITY_EDITOR
                        [CustomEditor(typeof(InventoryDropLayerRenderingManagementStrategy))]
                        [CanEditMultipleObjects]
                        public class InventoryDropLayerRenderingManagementStrategyEditor : Editor
                        {
                            SerializedProperty mainListener;
                            SerializedProperty mainListenerObject;
                            SerializedProperty mainListenerType;

                            protected virtual void OnEnable()
                            {
                                mainListener = serializedObject.FindProperty("mainListener");
                                mainListenerObject = mainListener.FindPropertyRelative("ObjectField");
                                mainListenerType = mainListener.FindPropertyRelative("ResultType");
                            }

                            public override void OnInspectorGUI()
                            {
                                serializedObject.Update();

                                InventoryDropLayerRenderingManagementStrategy underlyingObject = (serializedObject.targetObject as InventoryDropLayerRenderingManagementStrategy);
                                MonoBehaviour[] listeners = (from listener in underlyingObject.GetComponents<InventoryDropLayerRenderingManagementStrategy.RenderingListener>() select (MonoBehaviour)listener).ToArray();
                                GUIContent[] listenerNames = (from listener in listeners select new GUIContent(listener.GetType().Name)).ToArray();

                                int index = ArrayUtility.IndexOf(listeners, mainListenerObject.objectReferenceValue as MonoBehaviour);
                                index = EditorGUILayout.Popup(new GUIContent("Main Listener"), index, listenerNames);
                                mainListenerObject.objectReferenceValue = index >= 0 ? (listeners[index]) : null;
                                mainListenerType.stringValue = index >= 0 ? IUnifiedContainerBase.ConstructResolvedName(listeners[index].GetType()) : "";

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
#endif
                    }
                }
            }
        }
    }
}
