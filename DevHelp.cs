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

namespace DevHelp {
	public class DevHelp : Mod {
        internal static DevHelp instance;
        ModHotKey advancedTooltipsHotkey;
        ModHotKey recipeMakerTooltipsHotkey;
		public static bool readtooltips = false;

        public Texture2D[] buttonTextures;

		internal UserInterface UI;
        internal RecipeMakerUI recipeMakerUI;
        public override void Load() {
            instance = this;
			if (Main.netMode!=NetmodeID.Server){
				UI = new UserInterface();

                buttonTextures = new Texture2D[] {
                    GetTexture("Checkbox"),
                    GetTexture("Checkbox_Hovered"),
                    GetTexture("Checkbox_Selected"),
                    GetTexture("Checkbox_Selected_Hovered"),
                    GetTexture("Button_Copy"),
                    GetTexture("Button_Copy_Hovered")
                };
			}

            advancedTooltipsHotkey = RegisterHotKey("Toggle Advanced Tooltips", Keys.L.ToString());
            recipeMakerTooltipsHotkey = RegisterHotKey("Toggle Recipe Maker GUI", Keys.PageDown.ToString());
        }
        public override void Unload() {
            instance = null;
            buttonTextures = null;
        }
        public override void UpdateUI(GameTime gameTime) {
			UI?.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"DevHelp: RecipeMakerUI",
					delegate {
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						UI.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
        public override void HotKeyPressed(string name) {
            bool tick = false;
            if (advancedTooltipsHotkey?.JustPressed??false) {
                readtooltips = !readtooltips;
                tick = true;
            }
            if (recipeMakerTooltipsHotkey?.JustPressed??false) {
                if(recipeMakerUI is null){
                    recipeMakerUI = new RecipeMakerUI();
                    recipeMakerUI.Activate();
                    UI.SetState(recipeMakerUI);
                } else {
                    UI.SetState(recipeMakerUI = null);
                }
                tick = true;
            }
            if (tick) {
                Main.PlaySound(SoundID.MenuTick);
            }
        }
	}
}
