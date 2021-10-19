using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using System.Text;
using ReLogic.OS;

namespace DevHelp.UI {
	public class RecipeMakerUI : UIState {
        public List<RefItemSlot> materials = new List<RefItemSlot>(){};
        public List<RefItemSlot> tiles = new List<RefItemSlot>(){};
        public RefItemSlot outputItem;
        public UIButton copyButton;
        public override void OnInitialize(){
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
            materials.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                    Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                    Top = { Pixels = (float)(Main.screenHeight * 0.4) },
                    ValidItemFunc = item => true,
                    index = materials.Count
				});
            Append(materials[0]);

            tiles.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                    Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                    Top = { Pixels = (float)(Main.screenHeight * 0.4 + 45 * scale.Y) },
                    ValidItemFunc = item => true,
                    index = tiles.Count
				});
            Append(tiles[0]);

            outputItem = new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                    Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                    Top = { Pixels = (float)(Main.screenHeight * 0.4 - 45 * scale.Y) },
                    ValidItemFunc = item => true,
                    index = materials.Count
				};
            Append(outputItem);

            copyButton = new UIButton(DevHelp.instance.buttonTextures[4], DevHelp.instance.buttonTextures[5], 0.85f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05 - 26 * scale.X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 - 43 * scale.Y) },
                hover = () => Main.hoverItemName = "Copy to clipboard",
                function = () => {
                    Platform.Current.Clipboard = GenerateRecipe();
                    Main.NewText("Copied recipe to clipboard");
                }
            };
            Append(copyButton);
        }
        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
            int mCount = materials.Count;

            if (mCount > 1 && (materials[mCount-2].item?.Value?.IsAir??true)) {
                RemoveChild(materials[mCount-1]);
                materials.RemoveAt(mCount-1);
                if (materials[mCount-2].Left.Pixels + 40 * scale.X > Main.screenWidth * 0.4) {
                    foreach(RefItemSlot slot in tiles) {
                        slot.Top.Pixels -= 40 * scale.Y;
                    }
                }
            } else if (mCount > 0 && !(materials[mCount-1].item?.Value?.IsAir??true)) {
                bool loop = materials[mCount-1].Left.Pixels + 40 * scale.X > Main.screenWidth * 0.4;
                materials.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                    Left = { Pixels = (loop ? materials[0].Left.Pixels : materials[mCount-1].Left.Pixels + 40 * scale.X) },
                    Top = { Pixels = (loop ? materials[mCount-1].Top.Pixels + 40 * scale.Y : materials[mCount-1].Top.Pixels) },
                    ValidItemFunc = item => true,
                    index = materials.Count
				});
                Append(materials[mCount]);
                if (loop) {
                    foreach(RefItemSlot slot in tiles) {
                        slot.Top.Pixels += 40 * scale.Y;
                    }
                }
            }
            
            int tCount = tiles.Count;
            if (tCount > 1 && (tiles[tCount-2].item?.Value?.IsAir??true)) {
                RemoveChild(tiles[tCount-1]);
                tiles.RemoveAt(tCount-1);
            } else if (tCount > 0 && !(tiles[tCount-1].item?.Value?.IsAir??true)) {
                bool loop = tiles[tCount-1].Left.Pixels + 40 * scale.X > Main.screenWidth * 0.4;
                tiles.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                    Left = { Pixels = (loop ? tiles[0].Left.Pixels : tiles[tCount-1].Left.Pixels + 40 * scale.X) },
                    Top = { Pixels = (loop ? tiles[tCount-1].Top.Pixels + 40 * scale.Y : tiles[tCount-1].Top.Pixels) },
                    ValidItemFunc = item => item.IsAir || item.createTile >= TileID.Dirt,
                    index = tiles.Count
				});
                Append(tiles[tCount]);
            }
        }
        public override void OnDeactivate() {
            base.OnDeactivate();
            Main.LocalPlayer.QuickSpawnClonedItem(outputItem.item.Value, outputItem.item.Value.stack);
            foreach(RefItemSlot materialSlot in materials) {
                Main.LocalPlayer.QuickSpawnClonedItem(materialSlot.item.Value, materialSlot.item.Value.stack);
            }
            foreach(RefItemSlot tileSlot in tiles) {
                Main.LocalPlayer.QuickSpawnClonedItem(tileSlot.item.Value, tileSlot.item.Value.stack);
            }
        }
        public string GenerateRecipe() {
            StringBuilder output = new StringBuilder();
            output.Append("ModRecipe recipe = new ModRecipe(mod);\n");
            Dictionary<int, int> materialItems = new Dictionary<int, int>();
            Item item;
            for (int i = 0; i < materials.Count; i++) {
                item = materials[i]?.item?.Value;
                if (item?.IsAir??true) {
                    continue;
                }
                if (materialItems.ContainsKey(item.type)) {
                    materialItems[item.type] += item.stack;
                } else {
                    materialItems.Add(item.type, item.stack);
                }
            }
            for (int i = 0; i < materials.Count; i++) {
                item = materials[i]?.item?.Value;
                if (item?.IsAir??true) {
                    continue;
                }
                int count;
                if (materialItems[item.type] > 0) {
                    count = materialItems[item.type];
                    materialItems[item.type] = 0;
                } else {
                    continue;
                }
                output.Append($"recipe.AddIngredient({GetItemCodeName(item)}, {count});\n");
            }
            HashSet<int> tilesListed = new HashSet<int>();
            for (int i = 0; i < tiles.Count; i++) {
                item = tiles[i]?.item?.Value;
                if (item?.IsAir??true || tilesListed.Contains(item.createTile)) {
                    continue;
                }
                output.Append($"recipe.AddTile({GetTileCodeName(item.createTile)});\n");
            }
            output.Append($"recipe.SetResult({GetItemCodeName(outputItem?.item?.Value)}, {outputItem?.item?.Value?.stack});\n");
            output.Append("recipe.AddRecipe();");
            return output.ToString();
        }
        public static string GetItemCodeName(Item item) {
            if (item?.IsAir??true) {
                return "";
            }
            return item.type >= ItemID.Count ? $"ModContent.ItemType<{item.modItem.GetType().Name}>()" : $"ItemID.{ItemID.Search.GetName(item.type)}";
        }
        public static string GetTileCodeName(int tileID) {
            return tileID >= TileID.Count ? $"ModContent.ItemType<{ModContent.GetModTile(tileID).GetType().Name}>()" : $"TileID.{TileID.Search.GetName(tileID)}";
        }
    }
}