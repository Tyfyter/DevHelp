using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DevHelp.Commands
{
	public class DebugSpawnCommand : ModCommand {
		public override CommandType Type => CommandType.World;
		public override string Command => "spawn";
		public override string Usage => "/spawn (int)NPC\n/spawn mod NPC";
		public override string Description => "";

		public override void Action(CommandCaller player, string input, string[] args) {
			Vector2 pos = Main.netMode == NetmodeID.SinglePlayer ? Main.MouseWorld : player.Player.MountedCenter + new Vector2(128 * player.Player.direction, 0);
			int item;
			int givenitem = 0;
			int count = 1;
			if(args[0].ToLower()=="help"){
				string o = "";
				foreach(FieldInfo f in typeof(NPCID).GetFields()){
					if(f.Name.ToLower()!="count"){
						o+=f.Name+",";
						if(o.Length>60){
							player.Reply(o);
							o = "";
						}
					}
				}
				if(o.Length>0)player.Reply(o);
				player.Reply("Count:"+NPCID.Count);
				return;
			}else if(int.TryParse(args[0], out _)){
				if(!int.TryParse(args[0], out item))return;
				if(args.Length == 2)int.TryParse(args[1], out count);
				givenitem = NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)pos.X, (int)pos.Y, int.Parse(args[0]));
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)pos.X, (int)pos.Y, item);
			}else if(args.Length==1){
				Type type = typeof(NPCID);
				item = 0;
				if(type.GetField(args[0]) == null){
					ErrorMessage(player);
					return;
				}
				try{
					if(!int.TryParse(type.GetField(args[0]).GetRawConstantValue().ToString(),out item)) ErrorMessage(player);
				}catch(NullReferenceException){
					ErrorMessage(player);
					return;
				}
				if(args.Length == 2)int.TryParse(args[1], out count);
				givenitem = NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)pos.X, (int)pos.Y, item);
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)pos.X, (int)pos.Y, item);
			}/* else {
				Mod itemmod = ModLoader.GetMod(args[0]);
				if(itemmod==null)return;
				if(itemmod.GetNPC(args[1])==null)return;
				item = itemmod.NPCType(args[1]);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC(player.Player.GetSource_Misc("debug_spawn_command"), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
				//givenitem = Item.NewItem(player.Player.Center, new Vector2(), item, count, false, 0, true);
			}*/
			player.Reply("Player "+player.Player.name+" has successfully spawned "+Main.npc[givenitem].GivenOrTypeName+(count==1?"":"x"+count));
			//Main.NewText("Player "+player.Player.name+" was successfully given "+Main.item[givenitem].HoverName+" x"+count+"  [i/s"+count+":"+item+"]");
		}

		static void ErrorMessage(CommandCaller caller) {
			caller.Reply("Failed to spawn NPC. reason: invalid ID.", Color.OrangeRed);
			caller.Reply("Try \"/spawn help\" for a list of all NPC IDs.", Color.Orange);
		}
	}
	public class DebugGiveCommand : ModCommand {
		public override CommandType Type => CommandType.World;
		public override string Command => "give";
		public override string Usage => "/give (vanilla|null|terraria) <item> [count]\n/give <item> [count]\n/give <mod> <item> [count]";
		public override string Description => "";
		public override void Action(CommandCaller player, string input, string[] args)
		{
			int item = 0;
			int givenitem = 0;
			int count = 1;
			if(args[0].ToLower()=="help"){
				string o = "";
				foreach(FieldInfo f in typeof(ItemID).GetFields()){
					//if(f.Name.ToLower()!="count")Main.NewText(f.Name);
					if(f.Name.ToLower()!="count"){
						o+=f.Name+",";
						if(o.Length>60){
							player.Reply(o);
							o = "";
						}
					}
				}
				if(o.Length>0)player.Reply(o);
				player.Reply("Count:"+ItemID.Count);
				return;
			}else if(args[0]==""){
				Type type = typeof(ItemID);
				item = 0;
				if(!int.TryParse(type.GetField(args[1]).GetRawConstantValue().ToString(),out item)) ErrorMessage(player);//Main.NewText("Failed to give"+player.Player.name+" item. reason: invalid ID.", Color.OrangeRed);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = Item.NewItem(new EntitySource_DebugCommand(input), player.Player.Center, new Vector2(), item, count, false, 0, true);
			}else if(int.TryParse(args[0], out _)){
				if(!int.TryParse(args[0], out item))return;
				if(args.Length == 2)int.TryParse(args[1], out count);
            	givenitem = Item.NewItem(new EntitySource_DebugCommand(input), player.Player.Center, new Vector2(), int.Parse(args[0]), count, false, 0, true);
			}/*else{
				Mod itemmod = ModLoader.GetMod(args[0]);
				if(itemmod==null)return;
				if(itemmod.GetItem(args[1])==null)return;
				item = itemmod.ItemType(args[1]);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = Item.NewItem(player.Player.Center, new Vector2(), item, count, false, 0, true);
			}*/
			player.Reply("Player "+player.Player.name+" was successfully given "+Main.item[givenitem].HoverName+" x"+count+"  [i/s"+count+":"+item+"]");
		}

		static void ErrorMessage(CommandCaller caller) {
			caller.Reply("Failed to give Item. reason: invalid ID.", Color.OrangeRed);
			caller.Reply("Try \"/give help\" for a list of all Item IDs.", Color.Orange);
		}
	}
}