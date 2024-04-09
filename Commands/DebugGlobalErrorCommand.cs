using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using Mono.Cecil;
using MonoMod.Cil;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using Terraria.ModLoader.Core;

namespace DevHelp.Commands
{
	public class DebugGlobalErrorCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "listglobals"; }
		}

		public override string Usage
		{
			get { return "/listglobals <npc|item> <type> [hook]"; }
		}

		public override string Description 
		{
			get { return "outputs all globals affecting the npc/item, or all globals which hook the specified method for it"; }
		}

		public override void Action(CommandCaller player, string input, string[] args) {
			if (!int.TryParse(args[1], out int type)) {
				type = NPCID.Search.GetId(args[1]);
			}
			string hook = null;
			if (args.Length > 2) hook = args[2];
			switch (args[0].ToUpperInvariant()) {
				case "NPC": {
					NPC npc = NPC.NewNPCDirect(
						new EntitySource_Misc("fake"),
						0,
						0,
						type
					);
					if (hook is null) {
						foreach (GlobalNPC global in npc.Globals) {
							player.Reply(global.FullName);
						}
					} else {
						GlobalHookList<GlobalNPC> hooks = (GlobalHookList<GlobalNPC>)typeof(NPCLoader).GetField("Hook" + hook, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
						foreach (GlobalNPC global in hooks.Enumerate(npc)) {
							player.Reply(global.FullName);
						}
					}
					break;
				}
				case "ITEM": {
					Item item = Main.item[Item.NewItem(
						new EntitySource_Misc("fake"),
						default(Vector2),
						type
					)];
					if (hook is null) {
						foreach (GlobalItem global in item.Globals) {
							player.Reply(global.FullName);
						}
					} else {
						GlobalHookList<GlobalItem> hooks = (GlobalHookList<GlobalItem>)typeof(ItemLoader).GetField("Hook" + hook, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
						foreach (GlobalItem global in hooks.Enumerate(item)) {
							player.Reply(global.FullName);
						}
					}
					break;
				}
			}
		}
	}
}