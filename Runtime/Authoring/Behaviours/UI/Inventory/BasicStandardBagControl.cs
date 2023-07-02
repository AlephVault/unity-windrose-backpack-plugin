using System.Collections;
using System.Collections.Generic;
using AlephVault.Unity.Support.Authoring.Behaviours;
using UnityEngine;


namespace AlephVault.Unity.WindRose.BackPack
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace UI
            {
                namespace Inventory
                {
                    namespace StandardBag
                    {
                        using AlephVault.Unity.BackPack.Authoring.Behaviours.UI.Inventory.Basic;
                        using AlephVault.Unity.BackPack.Authoring.Behaviours.Inventory.Standard;
                        using AlephVault.Unity.BackPack.Types.Inventory.Stacks;
                        using Entities.Objects.Bags;

                        /// <summary>
                        ///   This is a basic control for the StandardBag component. It will have
                        ///     two involved parts: It will, to start, need a component of type
                        ///     <see cref="BasicStandardInventoryView" />, and a reference
                        ///     to a <see cref="StandardBag"/> component (which can be changed
                        ///     any time). On start, and/or when the reference is changed, the
                        ///     control's view will be connected to the start/new value of the
                        ///     bag's underlying invetory's rendering strategy. This implies
                        ///     that both objects belong to the same scope (e.g. local games)
                        ///     and also that these components will interact.
                        /// </summary>
                        [RequireComponent(typeof(BasicStandardInventoryLink))]
                        public class BasicStandardBagControl : MonoBehaviour
                        {
                            // The view component to perform the link.
                            private BasicStandardInventoryView inventoryView;
                            // The link component
                            private BasicStandardInventoryLink inventoryLink;

                            private void Awake()
                            {
                                inventoryView = GetComponent<BasicStandardInventoryView>();
                                inventoryLink = GetComponent<BasicStandardInventoryLink>();
                            }

                            /// <summary>
                            ///   Drops the currently selected item. Dropping it implies that the
                            ///     underlying <see cref="World.Layers.Drop.DropLayer"/> will get
                            ///     such dropped item, if the drop layer is in use. It will also
                            ///     refresh the related view to clear the selection, but because
                            ///     of the drop. This method does nothing if the inventory is not
                            ///     a bag.
                            /// </summary>
                            public void DropSelected()
                            {
                                StandardBag bag = inventoryLink.Inventory.GetComponent<StandardBag>();
                                if (!bag) return;

                                if (inventoryView.SelectedPosition != null)
                                {
                                    int position = inventoryView.SelectedPosition.Value;
                                    inventoryView.Unselect();
                                    bag.Drop(position);
                                    //inventoryView.Refresh();
                                }
                            }

                            /// <summary>
                            ///   Picks an item from the ground, if the map has a layer of type
                            ///     <see cref="World.Layers.Drop.DropLayer"/>. The just-picked
                            ///     item will be the selected one in the view. This method does
                            ///     nothing if the inventory is not a bag.
                            /// </summary>
                            public void Pick()
                            {
                                StandardBag bag = inventoryLink.Inventory.GetComponent<StandardBag>();
                                if (!bag) return;

                                int? finalPosition;
                                bag.Pick(out finalPosition);
                                if (finalPosition != null && inventoryView.SelectedPosition == null)
                                {
                                    inventoryView.Select(finalPosition.Value);
                                    inventoryView.Refresh();
                                }
                            }

                            /// <summary>
                            ///   Gets the currently selected stack, by considering both the
                            ///     selection in the UI and the content in the inventory.
                            /// </summary>
                            public Stack SelectedItem
                            {
                                get
                                {
                                    return (inventoryView.SelectedPosition != null) ? inventoryLink.Inventory.Find(inventoryView.SelectedPosition.Value) : null;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
