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
using Terraria.GameContent;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace DevHelp.UI {
	public class RecipeMakerUI : UIState {
        public List<RefItemSlot> materials = new List<RefItemSlot>(){};
        public List<RefItemSlot> tiles = new List<RefItemSlot>(){};
        public RefItemSlot outputItem;
        public UIButton copyButton;
        public UILabeledCheckbox alchemy;
        public UILabeledCheckbox snowBiome;
        public UILabeledCheckbox water;
        public UILabeledCheckbox lava;
        public UILabeledCheckbox honey;
		public UILabeledCheckbox[] otherConditions;
		public static FastStaticFieldInfo<Condition>[] conditionFields;
		public static FastStaticFieldInfo<Recipe[]> firstRecipeForItem;
		public override void OnInitialize(){
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
            materials.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                    colorContext: ItemSlot.Context.CraftingMaterial) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4) },
                ValidItemFunc = item => true,
                index = materials.Count,
                allowsRecipeGroups = true,
                extraTooltip = "Ingredient"
            });
            Append(materials[0]);

            tiles.Add(new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                colorContext: ItemSlot.Context.CraftingMaterial) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 + 45 * scale.Y) },
                ValidItemFunc = item => item.IsAir || item.createTile >= TileID.Dirt,
                index = tiles.Count,
                extraTooltip = "Required Tile"
            });
            Append(tiles[0]);

            outputItem = new RefItemSlot(scale: 0.75f, context: ItemSlot.Context.ChestItem,
                colorContext: ItemSlot.Context.CraftingMaterial) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 - 45 * scale.Y) },
                ValidItemFunc = item => true,
                index = materials.Count,
                extraTooltip = "Output Item"
            };
            Append(outputItem);

            copyButton = new UIButton(DevHelp.instance.buttonTextures[4], DevHelp.instance.buttonTextures[5], 0.85f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05 - 26 * scale.X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 - 43 * scale.Y) },
                hover = () => Main.hoverItemName = "Generate and copy to clipboard",
                function = () => {
                    Platform.Get<IClipboard>().Value = GenerateRecipe();
                    Main.NewText("Copied recipe to clipboard");
                }
            };
            Append(copyButton);

            alchemy = new UILabeledCheckbox("alchemy", 0.8f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05 + 45 * scale.X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 - 46 * scale.Y) }
            };
            Append(alchemy);

            snowBiome = new UILabeledCheckbox("needs snow biome", 0.8f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05 + 45 * scale.X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 - 26 * scale.Y) }
            };
            Append(snowBiome);
            
            water = new UILabeledCheckbox("needs water", 0.8f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 + 90 * scale.Y) }
            };
            Append(water);

            lava = new UILabeledCheckbox("needs lava", 0.8f) {
                Left = { Pixels = (float)(Main.screenWidth * 0.05 + 30 * scale.X + FontAssets.ItemStack.Value.MeasureString(water.label).X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 + 90 * scale.Y) }
            };
            Append(lava);

            honey = new UILabeledCheckbox("needs honey", 0.8f) {
                Left = { Pixels = (float)(lava.Left.Pixels + 30 * scale.X + FontAssets.ItemStack.Value.MeasureString(lava.label).X) },
                Top = { Pixels = (float)(Main.screenHeight * 0.4 + 90 * scale.Y) }
            };
            Append(honey);

			otherConditions = new UILabeledCheckbox[32];
			float maxX = 0;
			for (int i = 0; i < otherConditions.Length; i++) {
				otherConditions[i] = new UILabeledCheckbox(GetConditionName(i, false), 0.8f) {
					Left = { Pixels = Main.screenWidth * 0.05f },
					Top = { Pixels = (float)(Main.screenHeight * 0.4 + (110 + (i / 2) * 20) * scale.Y) }
				};
				Append(otherConditions[i]);
				float currentX = 30 * scale.X + FontAssets.ItemStack.Value.MeasureString(otherConditions[i].label).X;
				if (maxX < currentX) maxX = currentX;
			}
			for (int i = 1; i < otherConditions.Length; i += 2) {
				otherConditions[i].Left.Pixels += maxX;
			}
		}
		static string GetConditionName(int index, bool forCode) {
			if (conditionFields is null) {
				conditionFields = new FastStaticFieldInfo<Condition>[32];
			}
			string name = index switch {
				0  => "InBeach",
				1  => "InGranite",
				2  => "InMarble",
				3  => "InHive",
				4  => "InGemCave",
				5  => "InLihzhardTemple",
				6  => "InGraveyard",
				7  => "InUndergroundDesert",
				8  => "InDesert",
				9  => "InGlowshroom",
				10 => "InMeteor",
				11 => "InDungeon",
				12 => "InCorrupt",
				13 => "InHallow",
				14 => "InJungle",
				15 => "InCrimson",

				16 => "InSkyHeight",
				17 => "InOverworldHeight",
				18 => "InDirtLayerHeight",
				19 => "InRockLayerHeight",
				20 => "InUnderworldHeight",

				21 => "InRain",
				22 => "InSandstorm",
				23 => "InOldOneArmy",

				24 => "InWaterCandle",
				25 => "InPeaceCandle",

				26 => "InTowerSolar",
				27 => "InTowerVortex",
				28 => "InTowerNebula",
				29 => "InTowerStardust",

				30 => "TimeDay",
				31 => "TimeNight",
				_ => throw new ArgumentOutOfRangeException(nameof(index))
			};
			return forCode ? name : (conditionFields[index] ??= new(typeof(Condition), name, BindingFlags.Public)).GetValue().Description.Value; 
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
                    index = materials.Count,
                    allowsRecipeGroups = true,
                    extraTooltip = "Ingredient"
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
                    index = tiles.Count,
                    extraTooltip = "Required Tile"
                });
                Append(tiles[tCount]);
            }
        }
        public override void OnDeactivate() {
            base.OnDeactivate();
            Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), outputItem.item.Value, outputItem.item.Value.stack);
            foreach(RefItemSlot materialSlot in materials) {
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), materialSlot.item.Value, materialSlot.item.Value.stack);
            }
            foreach(RefItemSlot tileSlot in tiles) {
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), tileSlot.item.Value, tileSlot.item.Value.stack);
            }
        }
        public string GenerateRecipe() {
            StringBuilder output = new StringBuilder();
            output.Append($"Recipe.Create({GetItemCodeName(outputItem.item.Value)}, {outputItem.item.Value.stack})\n");
            Dictionary<int, int> materialItems = new Dictionary<int, int>();
            Dictionary<RecipeGroup, int> materialGroups = new Dictionary<RecipeGroup, int>();
            Item item;
            RecipeGroup recipeGroup;

			Recipe recipe = DevHelp.RecipeMakerRecipe;

			Recipe[] firstRecipeForItem = (RecipeMakerUI.firstRecipeForItem ??= new(typeof(RecipeLoader), "FirstRecipeForItem", BindingFlags.NonPublic)).GetValue();
			if (recipe.createItem is not null && firstRecipeForItem[recipe.createItem.type] == recipe) firstRecipeForItem[recipe.createItem.type] = null;
			recipe.Clear();
            recipe.ReplaceResult(outputItem.item.Value.type, outputItem.item.Value.stack);
			if (firstRecipeForItem[recipe.createItem.type] is null) firstRecipeForItem[recipe.createItem.type] = recipe;

			for (int i = 0; i < materials.Count; i++) {
                item = materials[i]?.item?.Value;
                if (item?.IsAir??true) {
                    continue;
                }

                if (materials[i].proxy) {
                    recipeGroup = materials[i].recipeGroup;
                    if (materialGroups.ContainsKey(recipeGroup)) {
                        materialGroups[recipeGroup] += item.stack;
                    } else {
                        materialGroups.Add(recipeGroup, item.stack);
                    }
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
                if (materials[i].proxy) {
                    recipeGroup = materials[i].recipeGroup;
                    if (materialGroups[recipeGroup] > 0) {
                        count = materialGroups[recipeGroup];
                        materialGroups[recipeGroup] = 0;
                    } else {
                        continue;
                    }
                    output.Append($".AddRecipeGroup({GetRecipeGroupCodeName(recipeGroup)}, {count})\n");
                    recipe.AddRecipeGroup(GetRecipeGroupCodeName(recipeGroup), count);
                    continue;
                }
                if (materialItems[item.type] > 0) {
                    count = materialItems[item.type];
                    materialItems[item.type] = 0;
                } else {
                    continue;
                }
                output.Append($".AddIngredient({GetItemCodeName(item)}, {count})\n");
                recipe.AddIngredient(item.type, count);
            }
            HashSet<int> tilesListed = new HashSet<int>();
            for (int i = 0; i < tiles.Count; i++) {
                item = tiles[i]?.item?.Value;
                if (item?.IsAir??true || tilesListed.Contains(item.createTile)) {
                    continue;
                }
                tilesListed.Add(item.createTile);
                output.Append($".AddTile({GetTileCodeName(item.createTile)})\n");
                recipe.AddTile(item.createTile);
            }
            if (alchemy.Checked) {
                output.Append($".AddConsumeItemCallback(Recipe.ConsumptionRules.Alchemy)\n");
                recipe.AddConsumeItemCallback(Recipe.ConsumptionRules.Alchemy);
            }
            if (water.Checked) {
                output.Append($".AddCondition(Recipe.Condition.NearWater)\n");
                recipe.AddCondition(Condition.NearWater);
            }
            if (lava.Checked) {
                output.Append($".AddCondition(Recipe.Condition.NearLava)\n");
                recipe.AddCondition(Condition.NearLava);
            }
            if (honey.Checked) {
                output.Append($".AddCondition(Recipe.Condition.NearHoney)\n");
                recipe.AddCondition(Condition.NearHoney);
            }
            if (snowBiome.Checked) {
                output.Append($".AddCondition(Recipe.Condition.InSnow)\n");
                recipe.AddCondition(Condition.InSnow);
            }
			for (int i = 0; i < otherConditions.Length; i++) {
				if (otherConditions[i].Checked) {
					output.Append($".AddCondition(Recipe.Condition.{GetConditionName(i, true)})\n");
					recipe.AddCondition(conditionFields[i].GetValue());
				}
			}
			DevHelp.RegenerateRequiredItemQuickLookup();
            output.Append(".Register();");
			return output.ToString();
		}
        public static string GetItemCodeName(Item item) {
            if (item?.IsAir??true) {
                return "";
            }
            return item.type >= ItemID.Count ? $"ModContent.ItemType<{item.ModItem.GetType().Name}>()" : $"ItemID.{ItemID.Search.GetName(item.type)}";
        }
        public static string GetTileCodeName(int tileID) {
            return tileID >= TileID.Count ? $"ModContent.ItemType<{ModContent.GetModTile(tileID).GetType().Name}>()" : $"TileID.{TileID.Search.GetName(tileID)}";
        }
        public static string GetRecipeGroupCodeName(RecipeGroup recipeGroup) {
            int index = -1;
            foreach (KeyValuePair<int, RecipeGroup> entry in RecipeGroup.recipeGroups) {
                if (recipeGroup == entry.Value) {
                    index = entry.Key;
                    break;
                }
            }
            if(index > 12){
                foreach (KeyValuePair<string, int> entry in RecipeGroup.recipeGroupIDs) {
                    if (index == entry.Value) {
                        return entry.Key;
                    }
                }
                return "";
            } else {
                
                switch (index) {
                    case 0: return "RecipeGroupID.Birds";
                    case 1: return "RecipeGroupID.Scorpions";
                    case 2: return "RecipeGroupID.Bugs";
                    case 3: return "RecipeGroupID.Ducks";
                    case 4: return "RecipeGroupID.Squirrels";
                    case 5: return "RecipeGroupID.Butterflies";
                    case 6: return "RecipeGroupID.Fireflies";
                    case 7: return "RecipeGroupID.Snails";
                    case 8: return "RecipeGroupID.Wood";
                    case 9: return "RecipeGroupID.IronBar";
                    case 10: return "RecipeGroupID.PressurePlate";
                    case 11: return "RecipeGroupID.Sand";
                    case 12: return "RecipeGroupID.Fragment";
                    default: return "RecipeGroupID.";
                }
            }
        }
    }
}