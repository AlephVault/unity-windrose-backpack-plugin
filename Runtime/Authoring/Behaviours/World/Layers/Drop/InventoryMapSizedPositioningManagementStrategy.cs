using System;
using System.Collections.Generic;
using UnityEngine;
using AlephVault.Unity.Support.Utils;

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
                        using AlephVault.Unity.Layout.Utils;
                        using AlephVault.Unity.BackPack.Authoring.Behaviours.Inventory.ManagementStrategies.PositioningStrategies;
                        using AlephVault.Unity.WindRose.Authoring.Behaviours.World;

                        /// <summary>
                        ///   This class validates and iterates position based on the map's dimensions.
                        ///     It will yield a <see cref="Vector2Int"/> for each pair of (x, y) position
                        ///     available in the map.
                        /// </summary>
                        public class InventoryMapSizedPositioningManagementStrategy : InventoryPositioningManagementStrategy
                        {
                            private uint width;
                            private uint height;

                            protected override void Awake()
                            {
                                base.Awake();
                                Map map = Behaviours.RequireComponentInParent<Map>(this);
                                width = map.Width;
                                height = map.Height;
                            }

                            /// <summary>
                            ///   Checks whether the position is a <see cref="Vector2Int"/> that is
                            ///     a valid position among the map.
                            /// </summary>
                            /// <param name="position">The position to check</param>
                            /// <returns>Whether the position is valid</returns>
                            protected override bool IsValid(object position)
                            {
                                if (position is Vector2Int)
                                {
                                    Vector2Int vector = (Vector2Int)position;
                                    return (Values.In(0, vector.x, (int?)(width - 1)) && Values.In(0, vector.y, (int?)(height - 1)));
                                }
                                return false;
                            }

                            /// <summary>
                            ///   Enumerates all the valid positions in the map. Each position
                            ///     will be a <see cref="Vector2Int"/>.
                            /// </summary>
                            /// <returns></returns>
                            public override IEnumerable<object> Positions()
                            {
                                for (var ix = 0; ix < width; ix++)
                                    for (var iy = 0; iy < height; iy++)
                                        yield return new Vector2Int(ix, iy);
                            }
                        }
                    }
                }
            }
        }
    }
}
