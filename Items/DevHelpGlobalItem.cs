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
            Player player = Main.player[item.owner];
            DevPlayer modPlayer = player.GetModPlayer<DevPlayer>();
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine tip;
                tip = tooltips[i];
                if(modPlayer.readtooltips){
                   tip.text = tip.Name + ": " + tip.text + "; " + tip.mod;
                }
                tooltips.RemoveAt(i);
                tooltips.Insert(i, tip);
            }
            if(modPlayer.readtooltips){
                if(item.modItem == null){
                    tooltips.Add(new TooltipLine(mod, "Advanced Tooltip", ItemID.Search.GetName(item.type)+":"+item.type));
                    if(item.dye!=0)tooltips.Add(new TooltipLine(mod, "Shader ID", "Shader ID: "+GameShaders.Armor.GetShaderIdFromItemId(item.type)));
                    return;
                }
                tooltips.Add(new TooltipLine(mod, "Advanced Tooltip", item.modItem.mod.Name+":"+item.modItem.Name+":"+item.type));
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