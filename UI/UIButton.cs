using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace DevHelp.UI
{
	// This class wraps the vanilla ItemSlot class into a UIElement. The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState.
	// By wrapping the vanilla ItemSlot class, we can easily use ItemSlot.
	// ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn.
	// If you want more control, you might need to write your own UIElement.
	// See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
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
	public class UILabeledCheckbox : UIElement {
		public Texture2D texture;
		public Texture2D hoverTexture;
		public Texture2D selectedTexture;
		public Texture2D selectedHoverTexture;
		private readonly float _scale = 1f;
		public bool Checked { get; set; }
		public UILabeledCheckbox(Texture2D texture, Texture2D hoverTexture,Texture2D selectedTexture, Texture2D selectedHoverTexture, float scale = 1f) {
			this.texture = texture;
			this.hoverTexture = hoverTexture ?? texture;
			this.texture = selectedTexture;
			this.hoverTexture = selectedHoverTexture ?? selectedTexture;
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
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					Checked = !Checked;
				}
			}
			spriteBatch.Draw(Checked ? (hovered ? selectedHoverTexture : selectedTexture) : (hovered ? hoverTexture : texture), bounds, Color.White);
			//Utils.DrawBorderStringFourWay();
			Main.inventoryScale = oldScale;
		}
	}
}