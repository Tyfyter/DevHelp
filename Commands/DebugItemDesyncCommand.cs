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

namespace DevHelp.Commands
{
	public class DebugItemDesyncCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "checkdesync"; }
		}

		public override string Usage
		{
			get { return "/checkdesync"; }
		}

		public override string Description 
		{
			get { return "outputs all items to text files"; }
		}

		public override void Action(CommandCaller player, string input, string[] args) {
			string folderPath = Path.Combine(Main.SavePath, "DevHelp", "ItemLists");
			if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
			foreach (Mod mod in ModLoader.Mods) {
				string filePath = Path.Combine(folderPath, mod.Name) + ".txt";
				StringBuilder builder = new();
				foreach (ModItem item in mod.GetContent<ModItem>()) {
					builder.AppendLine(item.Name);
				}
				File.WriteAllText(filePath, builder.ToString());
			}
			player.Reply("Wrote item lists to files");
		}
	}
	public class DebugItemLoadDesyncCommand : ModCommand {
		public override CommandType Type {
			get { return CommandType.Chat; }
		}

		public override string Command {
			get { return "finddesyncs"; }
		}

		public override string Usage {
			get { return "/finddesyncs"; }
		}

		public override string Description {
			get { return "lists content which may or may not load based on the presence of non-synced mods"; }
		}

		public override void Action(CommandCaller player, string input, string[] args) {
			player.Reply($"content at heavy risk of desync ({nameof(ModType.IsLoadingEnabled)} references non-synced mods): ");
			foreach (Mod mod in ModLoader.Mods) {
				if (!FindLoadEnabledDesyncs(mod, content => player.Reply(content.Name))) {
					player.Reply("None in ");
				}
			}
		}
		public static bool FindLoadEnabledDesyncs(Mod mod, Action<ModType> onFound) {
			MethodInfo IsLoadingEnabled = typeof(ModType).GetMethod(nameof(ModType.IsLoadingEnabled));
			bool foundAny = false;
			void Check<T>(T item) where T : ModType {
				if (item.GetType().Overrides(IsLoadingEnabled, out MethodInfo @override)) {
					List<Instruction> instructions = new(new DynamicMethodDefinition(@override).Definition.Body.Instructions);
					for (int i = 0; i < instructions.Count; i++) {
						if (instructions[i].MatchCall(typeof(ModLoader), nameof(ModLoader.HasMod)) && instructions[i - 1].MatchLdstr(out string modname)) {
							if (ModLoader.TryGetMod(modname, out Mod dependency) && dependency.Side != ModSide.Both) {
								foundAny = true;
								onFound(item);
								break;
							}
						}
					}
				}
			}
			foreach (ILoadable content in mod.GetContent()) {
				if (content is ModItem item) Check(item);
				else if (content is ModNPC npc) Check(npc);
				else if (content is ModBlockType tile) Check(tile);
				else if (content is ModBuff buff) Check(buff);
			}
			return foundAny;
		}
	}
}