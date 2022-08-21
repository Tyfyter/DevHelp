using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using DevHelp.UI;
using ReLogic.Content;
using Terraria.Audio;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Terraria.GameContent;
using Terraria.UI.Chat;
using System.IO;

namespace DevHelp {
	public class DevHelp : Mod {
        internal static DevHelp instance;
        public static ModKeybind AdvancedTooltipsHotkey { get; private set; }
        public static ModKeybind RecipeMakerHotkey { get; private set; }
        public static ModKeybind RarityImageHotkey { get; private set; }

        public static bool readtooltips = false;

        public AutoCastingAsset<Texture2D>[] buttonTextures;

		internal static UserInterface UI;
        internal static RecipeMakerUI recipeMakerUI;
        internal static int customRecipeIndex;
        public static Recipe RecipeMakerRecipe => Main.recipe[customRecipeIndex];
        public override void Load() {
            instance = this;
            if (Main.netMode!=NetmodeID.Server) {
				UI = new UserInterface();

                buttonTextures = new AutoCastingAsset<Texture2D>[] {
                    Assets.Request<Texture2D>("Checkbox"),
                    Assets.Request<Texture2D>("Checkbox_Hovered"),
                    Assets.Request<Texture2D>("Checkbox_Selected"),
                    Assets.Request<Texture2D>("Checkbox_Selected_Hovered"),
                    Assets.Request<Texture2D>("Button_Copy"),
                    Assets.Request<Texture2D>("Button_Copy_Hovered")
                };
			}

            AdvancedTooltipsHotkey = KeybindLoader.RegisterKeybind(this, "Toggle Advanced Tooltips", Keys.L.ToString());
            RecipeMakerHotkey = KeybindLoader.RegisterKeybind(this, "Toggle Recipe Maker GUI", Keys.PageDown.ToString());
            RarityImageHotkey = KeybindLoader.RegisterKeybind(this, "Save Rarity Name Image", Keys.PrintScreen.ToString());
        }
        public override void Unload() {
            instance = null;
            buttonTextures = null;
            AdvancedTooltipsHotkey = null;
            RecipeMakerHotkey = null;
            RarityImageHotkey = null;
            UI = null;
            recipeMakerUI = null;
        }
    }
    public class DevSystem : ModSystem {
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(ItemID.HallowedKey);
            recipe.AddIngredient(ItemID.HallowedKeyMold);
            recipe.Register();
            DevHelp.customRecipeIndex = recipe.RecipeIndex;
        }
		public override void UpdateUI(GameTime gameTime) {
            DevHelp.UI?.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1) {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "DevHelp: RecipeMakerUI",
                    delegate {
                        // If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
                        DevHelp.UI.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
    public class DevPlayer : ModPlayer {
        public static bool controlAdvancedTooltips;
        public static bool releaseAdvancedTooltips;
        public static bool controlRecipeMaker;
        public static bool releaseRecipeMaker;
        Func<int, ModRarity> _getRarity;
        Func<int, ModRarity> GetRarity => _getRarity ??= typeof(RarityLoader)
            .GetMethod("GetRarity", BindingFlags.NonPublic | BindingFlags.Static)
            .CreateDelegate<Func<int, ModRarity>>();
        public override void ProcessTriggers(TriggersSet triggersSet) {
            bool tick = false;

            //releaseAdvancedTooltips = !controlAdvancedTooltips;
            //controlAdvancedTooltips = triggersSet.KeyStatus["DevHelp: Toggle Advanced Tooltips"];
            if (DevHelp.AdvancedTooltipsHotkey.JustPressed) {
                DevHelp.readtooltips = !DevHelp.readtooltips;
                tick = true;
            }

            //releaseRecipeMaker = !controlRecipeMaker;
            //controlRecipeMaker = triggersSet.KeyStatus["DevHelp: Toggle Recipe Maker GUI"];
            if (DevHelp.RecipeMakerHotkey.JustPressed) {
                if (DevHelp.recipeMakerUI is null) {
                    DevHelp.recipeMakerUI = new RecipeMakerUI();
                    DevHelp.recipeMakerUI.Activate();
                    DevHelp.UI.SetState(DevHelp.recipeMakerUI);
                } else {
                    DevHelp.UI.SetState(DevHelp.recipeMakerUI = null);
                }
                tick = true;
            }

            if (DevHelp.RarityImageHotkey.JustPressed) {
                int rare = Main.HoverItem.rare;
                //Task.Run(()=> {
                Vector2 size = default;
                string text = null;
                Color color = default;
				if (GetRarity(rare) is ModRarity rarity) {
					if (rarity.GetType().GetProperty("RarityName") is PropertyInfo propertyInfo) {
                        text = (string)propertyInfo.GetValue(null);
					} else {
                        text = rarity.Name;
                    }
                    size = FontAssets.MouseText.Value.MeasureString(text);
                    Main.mouseTextColor = 255;
                    color = rarity.RarityColor;
                } else if(ItemRarityID.Search.TryGetName(rare, out string name)) {
                    size = FontAssets.MouseText.Value.MeasureString(name);
                    text = name;
                    color = ItemRarity.GetColor(rare);
                }
				if (text is not null) {
                    //GraphicsDevice graphicsDevice = new(Main.graphics.GraphicsDevice.Adapter, Main.graphics.GraphicsProfile, new PresentationParameters() {
                    //});
                    RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, (int)size.X + 8, (int)size.Y + 8);
                    SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
                    renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
                    renderTarget.GraphicsDevice.Clear(Color.Transparent);
                    spriteBatch.Begin();
                    SpriteBatch realMainSB = Main.spriteBatch;
					try {
                        Main.spriteBatch = spriteBatch;
                        int yoff = 0;
                        DrawableTooltipLine tooltipLine = new DrawableTooltipLine(
                            new TooltipLine(Mod, "ItemName", text),
                            0,
                            4,
                            4,
                            color
                        );
						if (ItemLoader.PreDrawTooltipLine(Main.HoverItem, tooltipLine, ref yoff)) {
                            ChatManager.DrawColorCodedStringWithShadow(
                                spriteBatch,
                                tooltipLine.Font,
                                tooltipLine.Text,
                                new Vector2(tooltipLine.X, tooltipLine.Y),
                                tooltipLine.Color,
                                tooltipLine.Rotation,
                                tooltipLine.Origin,
                                tooltipLine.BaseScale,
                                tooltipLine.MaxWidth,
                                tooltipLine.Spread
                            );
                        }
                    } finally {
                        Main.spriteBatch = realMainSB;
					}
                    spriteBatch.End();
                    string folderPath = Path.Combine(Main.SavePath, "DevHelp");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                    string filePath = Path.Combine(folderPath, "Rare" + text) + ".png";
                    Stream stream = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);
                    renderTarget.SaveAsPng(stream, (int)size.X + 8, (int)size.Y + 8);
                    renderTarget.GraphicsDevice.SetRenderTarget(null);
                    new System.Diagnostics.Process() {
                        StartInfo = new System.Diagnostics.ProcessStartInfo("explorer", filePath)
                    }.Start();
                    //Filters.Scene.EndCapture(null, Main.screenTarget, Main.screenTargetSwap, Color.Transparent);
                    //graphicsDevice.SetRenderTarget(null);
                }
				//});
                tick = true;
            }

            if (tick) {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }
    public struct AutoCastingAsset<T> where T : class {
        public bool HasValue => asset is not null;
        public bool IsLoaded => asset?.IsLoaded ?? false;
        public T Value => asset.Value;

        readonly Asset<T> asset;
        AutoCastingAsset(Asset<T> asset) {
            this.asset = asset;
        }
        public static implicit operator AutoCastingAsset<T>(Asset<T> asset) => new(asset);
        public static implicit operator T(AutoCastingAsset<T> asset) => asset.Value;
    }
    public static class DevExtensions {
        public static void Clear(this Recipe recipe) {
            recipe.requiredItem.Clear();
            recipe.requiredTile.Clear();
            recipe.Conditions.Clear();
            typeof(Recipe).GetProperty("ConsumeItemHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
            typeof(Recipe).GetProperty("OnCraftHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
        }
	}
}
