using UnityEngine;
using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.UI.Inventory.Basic;
using GameMeanMachine.Unity.BackPack.Authoring.Behaviours.Inventory.Standard;

namespace GameMeanMachine.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace UI
            {
                namespace Inventory
                {
                    /// <summary>
                    ///   Links an inventory and its related view through an
                    ///     exposed property to select the inventory. Once
                    ///     selected, the view will also be linked to it.
                    ///     Also, on component startup, if an inventory is
                    ///     selected, it will also link the view to it.
                    /// </summary>
                    [RequireComponent(typeof(BasicStandardInventoryView))]
                    public class BasicStandardInventoryLink : MonoBehaviour
                    {
                        // The view component to perform the link.
                        private BasicStandardInventoryView inventoryView;

                        /// <summary>
                        ///   The inventory this control will be bound to on start.
                        /// </summary>
                        [SerializeField]
                        private StandardInventory inventory;

                        private void Awake()
                        {
                            inventoryView = GetComponent<BasicStandardInventoryView>();
                        }

                        private void Start()
                        {
                            if (inventory) inventory.RenderingStrategy.Broadcaster.AddListener(inventoryView);
                        }

                        /// <summary>
                        ///   Sets or gets the current inventory this link is bound to. On change,
                        ///     the former inventory will not be watched anymore by this link, and
                        ///     the new one will start to be watched by this link.
                        /// </summary>
                        public StandardInventory Inventory
                        {
                            get
                            {
                                return inventory;
                            }
                            set
                            {
                                if (inventory) inventory.RenderingStrategy.Broadcaster.RemoveListener(inventoryView);
                                inventory = value;
                                if (inventory) inventory.RenderingStrategy.Broadcaster.AddListener(inventoryView);
                            }
                        }
                    }
                }
            }
        }
    }
}
