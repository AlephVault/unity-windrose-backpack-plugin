using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEditor;
using AlephVault.Unity.Support.Utils;
using AlephVault.Unity.MenuActions.Utils;

namespace GameMeanMachine.Unity.WindRose.BackPack
{
    namespace MenuActions
    {
        namespace Inventory
        {
            using Authoring.Behaviours.Drops;
            using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory.ManagementStrategies.UsageStrategies;
			using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory.ManagementStrategies.SpatialStrategies;
			using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory;
			using Authoring.Behaviours.World.Layers.Drop;

            /// <summary>
            ///   Menu actions to create drop/bag-related assets / add drop/bag-related components.
            /// </summary>
            public static class InventoryUtils
            {
                private class CreateDropContainerRendererPrefabWindow : EditorWindow
                {
                    private int imagesCount = 3;
                    private string prefabName = "DropDisplay";
                    public string prefabPath;

                    private void OnGUI()
                    {
                        minSize = new Vector2(360, 110);
                        maxSize = minSize;

                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        titleContent = new GUIContent("Wind Rose - Creating a new drop container renderer prefab");
                        EditorGUILayout.LabelField("This wizard will create a simple drop container renderer prefab, which is used in DropLayer's rendering strategy.", longLabelStyle);
                        EditorGUILayout.Separator();
                        prefabName = EditorGUILayout.TextField("Name", prefabName);
                        imagesCount = Values.Clamp(1, EditorGUILayout.IntField("Slots [1 to 32767]", imagesCount), 32767);
                        EditorGUILayout.Separator();
                        if (GUILayout.Button("Save"))
                        {
                            Execute();
                        }
                    }

                    private void Execute()
                    {
                        string relativePrefabPath = string.Format("{0}/{1}.prefab", prefabPath, prefabName);
                        GameObject prefab = new GameObject(prefabName);
                        prefab.AddComponent<SortingGroup>();
                        prefab.AddComponent<SimpleDropContainerRenderer>();
                        for(int i = 0; i < imagesCount; i++)
                        {
                            GameObject image = new GameObject("Img" + i);
                            image.AddComponent<SpriteRenderer>();
                            image.transform.parent = prefab.transform;
                        }
                        GameObject result = PrefabUtility.SaveAsPrefabAsset(prefab, relativePrefabPath);
                        Undo.RegisterCreatedObjectUndo(result, "Create Drop Container Renderer Prefab");
                        DestroyImmediate(prefab);
                        Close();
                        EditorUtility.DisplayDialog("Save Successful", "The drop container prefab was successfully saved. However, it can be configured to add/remove slots and/or set a background image.", "OK");
                    }
                }

                /// <summary>
                ///   This method is used in the assets menu action: Create > Back Pack > Inventory > Drop Container Renderer Prefab.
                /// </summary>
                [MenuItem("Assets/Create/Back Pack/Inventory/Drop Container Renderer Prefab")]
                public static void CreatePrefab()
                {
                    CreateDropContainerRendererPrefabWindow window = ScriptableObject.CreateInstance<CreateDropContainerRendererPrefabWindow>();
                    window.position = new Rect(new Vector2(230, 350), new Vector2(360, 110));
                    string newAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if (newAssetPath == "")
                    {
                        newAssetPath = Path.Combine("Assets", "Prefabs");
                    }
                    string projectPath = Path.GetDirectoryName(Application.dataPath);
                    if (!Directory.Exists(Path.Combine(projectPath, newAssetPath)))
                    {
                        newAssetPath = Path.GetDirectoryName(newAssetPath);
                    }
                    window.prefabPath = newAssetPath;
                    window.ShowUtility();
                }

                private class AddBagWindow : EditorWindow
                {
                    public Transform selectedTransform;
                    private bool finiteBag = true;
                    private int bagSize = 10;

                    private void OnGUI()
                    {
                        minSize = new Vector2(590, 144);
                        maxSize = new Vector2(590, 144);
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();

                        EditorGUILayout.LabelField("Bags involve a specific set of strategies like:\n" +
                                                   "> Simple spatial management strategy inside stack containers (finite or infinite, but slot-indexed)\n" +
                                                   "> Single-positioning strategy to locate stack containers (only one stack container: the bag)\n" +
                                                   "> Slot/Drop-styled rendering strategy\n" +
                                                   "All that contained inside a new inventory manager component. For the usage strategy, " +
                                                   "the NULL strategy will be added, which should be changed later or the items in the bag will have no logic.", longLabelStyle);
                        finiteBag = EditorGUILayout.ToggleLeft("Has a limited size", finiteBag);
                        if (finiteBag)
                        {
                            bagSize = EditorGUILayout.IntField("Bag size (>= 0)", bagSize);
                            if (bagSize < 1) bagSize = 1;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Bag size is potentially infinite.");
                        }
                        if (GUILayout.Button("Add a bag behaviour to the selected object"))
                        {
                            Execute();
                        }
                    }

                    private void Execute()
                    {
                        GameObject gameObject = selectedTransform.gameObject;
                        Undo.RegisterCompleteObjectUndo(gameObject, "Add Bag");
                        AlephVault.Unity.Layout.Utils.Behaviours.EnsureInactive(gameObject, delegate () {
                            InventoryNullUsageManagementStrategy usageStrategy = MenuActionUtils.AddUndoableComponent<InventoryNullUsageManagementStrategy>(gameObject);
                            InventoryManagementStrategyHolder holder = MenuActionUtils.AddUndoableComponent<InventoryManagementStrategyHolder>(gameObject, new Dictionary<string, object>() {
                                { "mainUsageStrategy", usageStrategy }
                            });
                            if (finiteBag)
                            {
                                InventoryFinite1DIndexedSpatialManagementStrategy simpleSpatialManagementStrategy = MenuActionUtils.AddUndoableComponent<InventoryFinite1DIndexedSpatialManagementStrategy>(gameObject, new Dictionary<string, object>() {
                                    { "size", bagSize }
                                });
                            }
                            else
                            {
                                MenuActionUtils.AddUndoableComponent<InventoryInfinite1DIndexedSpatialManagementStrategy>(gameObject);
                            }
                            MenuActionUtils.AddUndoableComponent<Authoring.Behaviours.Entities.Objects.Bags.StandardBag>(gameObject);
                            return holder;
                        });
                        Close();
                    }
                }

                /// <summary>
                ///   This method is used in the assets menu action: GameObject > Back Pack > Inventory > Add Bag.
                /// </summary>
                [MenuItem("GameObject/Back Pack/Inventory/Add Bag", false, 11)]
                public static void AddBag()
                {
                    AddBagWindow window = ScriptableObject.CreateInstance<AddBagWindow>();
                    window.selectedTransform = Selection.activeTransform;
                    window.position = new Rect(275, 327, 590, 144);
                    window.ShowUtility();
                }

                [MenuItem("GameObject/Back Pack/Inventory/Add Bag", true)]
                public static bool CanAddBag()
                {
                    return Selection.activeTransform && Selection.activeTransform.GetComponent<WindRose.Authoring.Behaviours.Entities.Objects.MapObject>();
                }

                private class AddDropLayerWindow : EditorWindow
                {
                    public Transform selectedTransform;

                    private void OnGUI()
                    {
                        minSize = new Vector2(564, 136);
                        maxSize = new Vector2(564, 136);
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();

                        EditorGUILayout.LabelField("Drop Layers involve a specific set of strategies like:\n" +
                                                   "> Infinite simple spatial management strategy inside stack containers\n" +
                                                   "> Map-sized positioning strategy to locate stack containers\n" +
                                                   "> Drop-styled rendering strategy\n" +
                                                   "All that contained inside a new inventory manager component. For the usage strategy, " +
                                                   "the NULL strategy will be added (which usually works for most games), but it may be changed later.", longLabelStyle);
                        EditorGUILayout.LabelField("No drop container renderer prefab will automatically be used or generated in the rendering strategy. " +
                                                   "One MUST be created/reused later.", captionLabelStyle);
                        if (GUILayout.Button("Add a drop layer to the selected map"))
                        {
                            Execute();
                        }
                    }

                    private void Execute()
                    {
                        GameObject dropLayer = new GameObject("DropLayer");
                        dropLayer.transform.parent = selectedTransform;
                        dropLayer.SetActive(false);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<SortingGroup>(dropLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<AlephVault.Unity.Support.Authoring.Behaviours.Normalized>(dropLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<InventoryInfinite1DIndexedSpatialManagementStrategy>(dropLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<InventoryMapSizedPositioningManagementStrategy>(dropLayer);
                        InventoryNullUsageManagementStrategy usageStrategy = AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<InventoryNullUsageManagementStrategy>(dropLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<InventoryDropLayerRenderingManagementStrategy>(dropLayer);
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<InventoryManagementStrategyHolder>(dropLayer, new Dictionary<string, object>() {
                            { "mainUsageStrategy", usageStrategy }
                        });
                        AlephVault.Unity.Layout.Utils.Behaviours.AddComponent<DropLayer>(dropLayer);
                        dropLayer.SetActive(true);
                        Undo.RegisterCreatedObjectUndo(dropLayer, "Add Drop Layer");
                        Close();
                    } 
                }

                /// <summary>
                ///   This method is used in the assets menu action: GameObject > Back Pack > Inventory > Add Drop Layer.
                /// </summary>
                [MenuItem("GameObject/Back Pack/Inventory/Add Drop Layer", false, 11)]
                public static void AddDropLayer()
                {
                    AddDropLayerWindow window = ScriptableObject.CreateInstance<AddDropLayerWindow>();
                    window.selectedTransform = Selection.activeTransform;
                    window.position = new Rect(264, 333, 564, 136);
                    window.ShowUtility();
                }

                [MenuItem("GameObject/Back Pack/Inventory/Add Drop Layer", true)]
                public static bool CanAddDropLayer()
                {
                    return Selection.activeTransform && Selection.activeTransform.GetComponent<WindRose.Authoring.Behaviours.World.Map>();
                }
            }
        }
    }
}