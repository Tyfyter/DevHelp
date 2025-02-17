using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace DevHelp {
	public class ShowInternalNames : GlobalNPC {
		public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(new InternalNameElement(NPCID.Search.GetName(npc.netID)));
		}
	}
	public class InternalNameElement(string internalName) : IBestiaryInfoElement {
		public UIElement ProvideUIElement(BestiaryUICollectionInfo info) {
			if (!DevHelpConfig.Instance.showNamesInBestiary) return null;
			UIElement uIElement = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7) {
				Width = new StyleDimension(-14f, 1f),
				Height = new StyleDimension(34f, 0f),
				BackgroundColor = new Color(43, 56, 101),
				BorderColor = Color.Transparent,
				Left = new StyleDimension(5f, 0f)
			};
			uIElement.SetPadding(0f);
			uIElement.PaddingRight = 5f;
			UIText element = new(internalName, 0.8f) {
				HAlign = 0f,
				Left = new StyleDimension(4f, 0f),
				TextOriginX = 0f,
				VAlign = 0.5f,
				DynamicallyScaleDownToWidth = true
			};
			uIElement.Append(element);
			uIElement.OnUpdate += static element => {
				if (element.IsMouseHovering) {
					string textValue = "Right click to copy";
					Main.instance.MouseText(textValue, 0, 0);
				}
			};
			uIElement.OnRightClick += (_, _) => {
				Platform.Get<IClipboard>().Value = ItemSlot.ShiftInUse ? internalName.Split("/")[^1] : internalName;
				Main.NewText("Copied to clipboard");
			};
			return uIElement;
		}
	}
}
