using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using System.Linq;

namespace DevHelp.Commands {
	public class DebugGlobalErrorCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "listglobals";
		public override string Usage => "/listglobals <npc|item|projectile> <type> [hook]";
		public override string Description => "outputs all globals affecting the npc/item, or all globals which hook the specified method for it";
		public override void Action(CommandCaller player, string input, string[] args) {
			int type;
			string hook = null;
			if (args.Length > 2) hook = args[2];
			switch (args[0].ToUpperInvariant()) {
				case "NPC": {
					if (!int.TryParse(args[1], out type)) {
						type = NPCID.Search.GetId(args[1]);
					}
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
					if (!int.TryParse(args[1], out type)) {
						type = ItemID.Search.GetId(args[1]);
					}
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
				case "PROJECTILE": {
					if (!int.TryParse(args[1], out type)) {
						type = ProjectileID.Search.GetId(args[1]);
					}
					Projectile item = Main.projectile[Projectile.NewProjectile(
						new EntitySource_Misc("fake"),
						default,
						default,
						type,
						0,
						0
					)];
					if (hook is null) {
						foreach (GlobalProjectile global in item.Globals) {
							player.Reply(global.FullName);
						}
					} else {
						GlobalHookList<GlobalProjectile> hooks = (GlobalHookList<GlobalProjectile>)typeof(ProjectileLoader).GetField("Hook" + hook, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
						foreach (GlobalProjectile global in hooks.Enumerate(item)) {
							player.Reply(global.FullName);
						}
					}
					break;
				}
			}
		}
	}
	public class DebugMisflowCommand : ModCommand {
		public override CommandType Type => CommandType.Chat;
		public override string Command => "misflow";
		public override string Usage => "/misflow <npc|item|projectile> <type>";
		public override string Description => "";
		public override void Action(CommandCaller player, string input, string[] args) {
			if (args[1].ToUpperInvariant() == "EVERYTHING") {
				bool everythingIsFine = true;
				int length = 0;
				switch (args[0].ToUpperInvariant()) {
					case "NPC":
					length = NPCLoader.NPCCount;
					break;

					case "ITEM":
					length = ItemLoader.ItemCount;
					break;

					case "PROJECTILE":
					length = ProjectileLoader.ProjectileCount;
					break;
				}
				for (int i = 0; i < length; i++) {
					args[1] = i.ToString();
					if (!ProcessForArgs(player, args)) {
						everythingIsFine = false;
					}
				}
				if (everythingIsFine) {
					player.Reply($"Everything is fine");
				}
			} else if (args[1].ToUpperInvariant() == "ACTIVE") {
				bool everythingIsFine = true;
				IEnumerable<Entity> enumerable = null;
				switch (args[0].ToUpperInvariant()) {
					case "NPC":
					enumerable = Main.npc.Where(e => e.active);
					break;

					case "ITEM":
					enumerable = Main.item.Where(e => e.active);
					break;

					case "PROJECTILE":
					enumerable = Main.projectile.Where(e => e.active);
					break;
				}
				foreach (Entity entity in enumerable) {
					List<(string globalName, string entityName, Action<BitWriter, BinaryWriter> send, Action<BitReader, BinaryReader> recieve)> functions = new();
					if (!ProcessList(player, functions)) {
						everythingIsFine = false;
					}
				}
				if (everythingIsFine) {
					player.Reply($"Everything active is fine");
				}
			} else {
				if (ProcessForArgs(player, args)) {
					player.Reply($"Everything is fine");
				}
			}
		}
		static void FillFromEntity(List<(string globalName, string entityName, Action<BitWriter, BinaryWriter> send, Action<BitReader, BinaryReader> recieve)> functions, Entity entity) {
			if (entity is NPC npc) {
				foreach (GlobalNPC global in npc.Globals) {
					functions.Add((
						global.FullName,
						npc.FullName,
						(bit, bin) => global.SendExtraAI(npc, bit, bin),
						(bit, bin) => global.ReceiveExtraAI(npc, bit, bin)
					));
				}
				return;
			}
			if (entity is Item item) {
				foreach (GlobalItem global in item.Globals) {
					functions.Add((
						global.FullName,
						item.Name,
						(_, bin) => global.NetSend(item, bin),
						(_, bin) => global.NetReceive(item, bin)
					));
				}
				return;
			}
			if (entity is Projectile projectile) {
				foreach (GlobalProjectile global in projectile.Globals) {
					functions.Add((
						global.FullName,
						projectile.Name,
						(bit, bin) => global.SendExtraAI(projectile, bit, bin),
						(bit, bin) => global.ReceiveExtraAI(projectile, bit, bin)
					));
				}
				return;
			}
			throw new ArgumentException($"Invalid argument type \"{entity.GetType()}\"", nameof(entity));
		}
		static bool ProcessForArgs(CommandCaller player, string[] args) {
			int type;
			List<(string globalName, string entityName, Action<BitWriter, BinaryWriter> send, Action<BitReader, BinaryReader> recieve)> functions = new();
			switch (args[0].ToUpperInvariant()) {
				case "NPC": {
					if (!int.TryParse(args[1], out type)) {
						type = NPCID.Search.GetId(args[1]);
					}
					NPC npc = new();
					npc.SetDefaults(type);
					FillFromEntity(functions, npc);
					break;
				}
				case "ITEM": {
					if (!int.TryParse(args[1], out type)) {
						type = ItemID.Search.GetId(args[1]);
					}
					Item item = new();
					item.SetDefaults(type);
					FillFromEntity(functions, item);
					break;
				}
				case "PROJECTILE": {
					if (!int.TryParse(args[1], out type)) {
						type = ProjectileID.Search.GetId(args[1]);
					}
					Projectile projectile = new();
					projectile.SetDefaults(type);
					FillFromEntity(functions, projectile);
					break;
				}
			}
			return ProcessList(player, functions);
		}
		static bool ProcessList(CommandCaller player, List<(string globalName, string entityName, Action<BitWriter, BinaryWriter> send, Action<BitReader, BinaryReader> recieve)> functions) {
			bool everythingIsFine = true;
			foreach (var (globalName, entityName, send, recieve) in functions) {
				MemoryStream stream = new();

				BinaryWriter writer = new(stream);
				BitWriter bitWriter = new();
				send(bitWriter, writer);
				bitWriter.Flush(writer);

				long position = stream.Position;
				stream.Position = 0;
				BinaryReader reader = new(stream);
				BitReader bitReader = new(reader);
				recieve(bitReader, reader);

				if (position != stream.Position) {
					everythingIsFine = false;
					player.Reply($"{globalName} on {entityName}: writes {position - stream.Position} more than it reads");
				}
			}
			return everythingIsFine;
		}
	}
}