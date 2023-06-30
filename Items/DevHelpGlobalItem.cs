using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using System.Reflection;
using Terraria.Audio;
using Terraria.Graphics.Shaders;

namespace DevHelp.Items {
	public class DevHelpGlobalItem : GlobalItem {
		internal static string recipeMakerTooltip = null;
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (recipeMakerTooltip is not null) {
				tooltips.Insert(0, new TooltipLine(Mod, "RecipeMakerSlot", recipeMakerTooltip+":"));
				recipeMakerTooltip = null;
			}
			if (DevHelp.readtooltips) {
				for (int i = 0; i < tooltips.Count; i++) {
					TooltipLine tip;
					tip = tooltips[i];
						tip.Text = tip.Name + ": " + tip.Text;
						switch (tip.Name) {
							case "Speed":
							tip.Text += $" ({CombinedHooks.TotalAnimationTime(item.useAnimation, Main.LocalPlayer, item)}/{CombinedHooks.TotalUseTime(item.useTime, Main.LocalPlayer, item)})";
							break;
							case "Knockback":
							tip.Text += $" ({Main.LocalPlayer.GetWeaponKnockback(item):0.##})";
							break;
						}
						if (tip.Mod != "Terraria") tip.Text += "; " + tip.Mod;
				
					tooltips.RemoveAt(i);
					tooltips.Insert(i, tip);
				}
				if (item.ModItem == null) {
					tooltips.Add(new TooltipLine(Mod, "Advanced Tooltip", ItemID.Search.GetName(item.type)+":"+item.type));
					if (item.dye != 0) tooltips.Add(new TooltipLine(Mod, "Shader ID", "Shader ID: "+GameShaders.Armor.GetShaderIdFromItemId(item.type)));
				} else {
					tooltips.Add(new TooltipLine(Mod, "Advanced Tooltip", item.ModItem.Mod.Name + ":" + item.ModItem.Name + ":" + item.type));
				}
			}
		}
		/*public override void SetDefaults(Item item) {
			base.SetDefaults(item);
			if(item.type==ItemID.ClothierVoodooDoll) {
				item.useStyle = ItemUseStyleID.HoldingUp;
				item.consumable = true;
			}
		}
		public override bool UseItem(Item item, Player player) {
			if(item.type==ItemID.ClothierVoodooDoll&&!Main.dayTime)NPC.SpawnOnPlayer(player.whoAmI,NPCID.SkeletronHead);
			return base.UseItem(item,player);
		}*/
	}
}