using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace DevHelp.UI {
	public class UIButton : UIElement {
		public Texture2D texture;
		public Texture2D hoverTexture;
		private readonly float _scale = 1f;
		internal Action function;
		internal Action hover;
		public UIButton(Texture2D texture, Texture2D hoverTexture, float scale = 1f) {
			this.texture = texture;
			this.hoverTexture = hoverTexture ?? texture;
			//_scale = scale;
			Width.Set(texture.Width * scale, 0f);
			Height.Set(texture.Height * scale, 0f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle bounds = GetDimensions().ToRectangle();
			bool hovered = false;
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				hovered = true;
				hover();
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					function();
				}
			}
			spriteBatch.Draw(hovered ? hoverTexture : texture, bounds, Color.White);
			Main.inventoryScale = oldScale;
		}
	}
	public class UIIconButton : GroupOptionButton<bool> {
		static FastFieldInfo<GroupOptionButton<bool>, Asset<Texture2D>> _iconTexture = new("_iconTexture", BindingFlags.NonPublic);
		public UIIconButton(Asset<Texture2D> icon, LocalizedText hoverText) : base(true, null, hoverText, Color.White, null) {
			_iconTexture.SetValue(this, icon);
			Width.Set(34, 0);
			SetColorsBasedOnSelectionState(Colors.FancyUIFatButtonMouseOver, Colors.InventoryDefaultColor, 0.7f, 0.7f);
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (IsMouseHovering) {
				Main.LocalPlayer.mouseInterface = true;
			}
		}
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			if (IsMouseHovering) {
				UICommon.TooltipMouseText(Description.Value);
			}
		}
	}
	public class UILabeledCheckbox : UIElement {
		public string label;
		private readonly float _scale = 1f;
		public bool Checked { get; set; }
		public UILabeledCheckbox(string text, float scale = 1f) {
			label = text;
			//_scale = scale;
			Width.Set(DevHelp.instance.buttonTextures[0].Value.Width * scale, 0f);
			Height.Set(DevHelp.instance.buttonTextures[0].Value.Height * scale, 0f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle bounds = GetDimensions().ToRectangle();
			int texture = Checked ? 2 : 0;
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				texture |= 1;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Checked = !Checked;
				}
			}
			spriteBatch.Draw(DevHelp.instance.buttonTextures[texture], bounds, Color.White);
			Vector2 size = FontAssets.ItemStack.Value.MeasureString(label);
			Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, label, bounds.X + bounds.Width + 5, bounds.Y + bounds.Height * 0.6f, Color.White, Color.Black, new Vector2(0, size.Y / 2));
			Main.inventoryScale = oldScale;
		}
	}
}