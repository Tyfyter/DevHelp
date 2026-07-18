using Microsoft.Xna.Framework;
using System.Diagnostics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace DevHelp.UI {
	public class BuffSelectorUI : UIState {
		[Range(0, 60 * 60)]
		public int duration = 15;
		public override void OnInitialize() {
			int row = 0, col = 0;
			int top = Main.screenHeight / 5;
			UIElement time = ConfigManager.WrapIt(this, ref top, new(GetType().GetField(nameof(duration))), this, 0).Item2;
			RemoveAllChildren();
			Height.Set(0, 1);
			Width.Set(0, 1);
			time.Left.Set(0, 0.025f);
			time.Width.Set(36 * 20, 0);
			time.Top.Set(-(time.Height.Pixels + 2), 0.15f);
			time.OnUpdate += _ => {
				if (time.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
			};
			Append(time);
			UIScrollableRegion scrollableRegion = new(1) {
				Left = new(0, 0.025f),
				Top = new(0, 0.15f),
				Width = new(0, 1f),
				Height = new(0, 0.7f)
			};
			for (int i = 1; i < BuffLoader.BuffCount; i++) {
				int type = i;
				UIImageButton iconButton = new(TextureAssets.Buff[i]) {
					Left = new(36 * col, 0),
					Top = new(36 * row, 0)
				};
				iconButton.OnLeftClick += (_, _) => {
					Main.LocalPlayer.AddBuff(type, duration * 60);
				};
				Color color = Main.debuff[type] ? Color.Red : Color.Lime;
				string tooltip = $"[c/{color.Hex3()}:{Lang.GetBuffName(type)}]\n{Lang.GetBuffDescription(type)}";
				iconButton.OnUpdate += _ => {
					if (!Main.LocalPlayer.mouseInterface && iconButton.IsMouseHovering) {
						Main.instance.MouseText(tooltip);
						Main.LocalPlayer.mouseInterface = true;
					}
				};
				scrollableRegion.Append(iconButton);
				if (++col >= 20) {
					col = 0;
					row++;
				}
			}
			Append(scrollableRegion);
		}
		public class UIScrollableRegion : UIElement {
			float scrolltiplier;
			public UIScrollableRegion(float scrolltiplier) {
				OverflowHidden = true;
				this.scrolltiplier = scrolltiplier;
			}
			public override void ScrollWheel(UIScrollWheelEvent evt) {
				base.ScrollWheel(evt);
				float maxScrollUp = 0;
				float maxScrollDown = 0;
				CalculatedStyle ownDimensions = GetInnerDimensions();
				for (int i = 0; i < Elements.Count; i++) {
					CalculatedStyle dimensions = Elements[i].GetOuterDimensions();
					float top = ownDimensions.Y - dimensions.Y;
					float bottom = (dimensions.Y + dimensions.Height) - (ownDimensions.Y + ownDimensions.Height);
					if (top > maxScrollUp) maxScrollUp = top;
					if (bottom > maxScrollDown) maxScrollDown = bottom;
				}
				float offset = float.Clamp(evt.ScrollWheelValue * scrolltiplier, -maxScrollDown, maxScrollUp);
				for (int i = 0; i < Elements.Count; i++) {
					Elements[i].Top.Pixels += offset;
					Elements[i].Recalculate();
				}
			}
		}
	}
}