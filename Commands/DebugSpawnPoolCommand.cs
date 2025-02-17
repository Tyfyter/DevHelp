/*using Ionic.Zlib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace DevHelp.Commands
{
	public class DebugSpawnPoolCommand : ModCommand {
		Type ChestLootLoader;
		public override bool IsLoadingEnabled(Mod mod) {
			ChestLootLoader = typeof(ItemLoader).Assembly.GetType("Terraria.ModLoader.ChestLootLoader");
			return ChestLootLoader is not null;
		}
		public override CommandType Type => CommandType.World;
		public override string Command => "drop";
		public override string Usage => "/drop (int)NPC\n/spawn mod NPC";
		public override string Description => "";
		[NoJIT]
		public override void Action(CommandCaller caller, string input, string[] args) {
			DropAttemptInfo dropAttemptInfo = new() {
				player = caller.Player,
				chest = Main.chest.IndexInRange(caller.Player.chest) ? Main.chest[caller.Player.chest]  : null,
				IsExpertMode = Main.expertMode,
				IsMasterMode = Main.masterMode,
				rng = Main.rand
			};
			if (args[0].Equals("loot", StringComparison.CurrentCultureIgnoreCase)) {
				if (!args[1].Equals("help", StringComparison.CurrentCultureIgnoreCase)) {
					if (Terraria.ModLoader.ChestLootLoader.GetLootPool(args[1]) is List<IItemDropRule> rules) {
						foreach (IItemDropRule rule in rules) {
							ItemDropResolver.ResolveRule(rule, dropAttemptInfo);
						}
						caller.Reply($"Player {caller.Player.name} has successfully dropped loot pool {args[1]}");
					} else {
						ErrorMessage(caller);
					}
				} else {
					foreach (KeyValuePair<string, List<IItemDropRule>> item in (Dictionary<string, List<IItemDropRule>>)ChestLootLoader.GetField("lootPools", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)) {
						caller.Reply(item.Key);
					}
				}
			} else if (args[0].Equals("item", StringComparison.CurrentCultureIgnoreCase)) {
				if (!args[1].Equals("help", StringComparison.CurrentCultureIgnoreCase)) {
					if (Terraria.ModLoader.ChestLootLoader.GetItemPool(args[1]) is not null) {
						ItemDropResolver.ResolveRule(new DropFromItemPoolRule(args[1]), dropAttemptInfo);
						caller.Reply($"Player {caller.Player.name} has successfully dropped item pool {args[1]}");
					} else {
						ErrorMessage(caller);
					}
				} else {
					object itemPool = ChestLootLoader.GetField("lootPools", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
					object keys = itemPool.GetType().GetProperty("Keys").GetValue(itemPool);
					string[] names = new string[(int)keys.GetType().GetProperty("Count").GetValue(keys)];
					keys.GetType().GetMethod("CopyTo").Invoke(keys, [names, 0]);
					foreach (string item in names) {
						caller.Reply(item);
					}
				}
			} else {
				caller.Reply("Failed to drop. reason: invalid mode.", Color.OrangeRed);
				caller.Reply("Try \"/drop (loot|item) <name>\".", Color.Orange);
			}
		}

		static void ErrorMessage(CommandCaller caller) {
			caller.Reply("Failed to drop. reason: invalid ID.", Color.OrangeRed);
			caller.Reply("Try \"/drop (loot|item) help\" for a list of all loot/item pool names.", Color.Orange);
		}
	}
}*/