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

namespace DevHelp {
	public class DevHelp : Mod {
        internal static DevHelp instance;
        public static ModKeybind AdvancedTooltipsHotkey { get; private set; }
        public static ModKeybind RecipeMakerHotkey { get; private set; }

		public static bool readtooltips = false;

        public AutoCastingAsset<Texture2D>[] buttonTextures;

		internal static UserInterface UI;
        internal static RecipeMakerUI recipeMakerUI;
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
        }
        public override void Unload() {
            instance = null;
            buttonTextures = null;
            AdvancedTooltipsHotkey = null;
            RecipeMakerHotkey = null;
            UI = null;
            recipeMakerUI = null;
        }
    }
    public class DevSystem : ModSystem {
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
        public override void ProcessTriggers(TriggersSet triggersSet) {
            bool tick = false;

            releaseAdvancedTooltips = !controlAdvancedTooltips;
            controlAdvancedTooltips = triggersSet.KeyStatus["DevHelp: Toggle Advanced Tooltips"];
            if (controlAdvancedTooltips && releaseAdvancedTooltips) {
                DevHelp.readtooltips = !DevHelp.readtooltips;
                tick = true;
            }

            releaseRecipeMaker = !controlRecipeMaker;
            controlRecipeMaker = triggersSet.KeyStatus["DevHelp: Toggle Recipe Maker GUI"];
            if (controlRecipeMaker && releaseRecipeMaker) {
                if (DevHelp.recipeMakerUI is null) {
                    DevHelp.recipeMakerUI = new RecipeMakerUI();
                    DevHelp.recipeMakerUI.Activate();
                    DevHelp.UI.SetState(DevHelp.recipeMakerUI);
                } else {
                    DevHelp.UI.SetState(DevHelp.recipeMakerUI = null);
                }
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
}
