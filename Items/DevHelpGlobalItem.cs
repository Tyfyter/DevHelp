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

namespace DevHelp.Items
{
	public class DevHelpGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine tip;
                tip = tooltips[i];
                if(DevHelp.readtooltips){
                   tip.Text = tip.Name + ": " + tip.Text + "; " + tip.Mod;
                }
                tooltips.RemoveAt(i);
                tooltips.Insert(i, tip);
            }
            if(DevHelp.readtooltips){
                if(item.ModItem == null){
                    tooltips.Add(new TooltipLine(Mod, "Advanced Tooltip", ItemID.Search.GetName(item.type)+":"+item.type));
                    if(item.dye!=0)tooltips.Add(new TooltipLine(Mod, "Shader ID", "Shader ID: "+GameShaders.Armor.GetShaderIdFromItemId(item.type)));
                    return;
                }
                tooltips.Add(new TooltipLine(Mod, "Advanced Tooltip", item.ModItem.Mod.Name+":"+item.ModItem.Name+":"+item.type));
            }
        }
        /*public override void SetDefaults(Item item){
            base.SetDefaults(item);
            if(item.type==ItemID.ClothierVoodooDoll){
                item.useStyle = ItemUseStyleID.HoldingUp;
                item.consumable = true;
            }
        }
        public override bool UseItem(Item item, Player player){
            if(item.type==ItemID.ClothierVoodooDoll&&!Main.dayTime)NPC.SpawnOnPlayer(player.whoAmI,NPCID.SkeletronHead);
            return base.UseItem(item,player);
        }*/
	}
}