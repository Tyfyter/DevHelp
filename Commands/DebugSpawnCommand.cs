using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DevHelp.Commands
{
	public class DebugSpawnCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "spawn"; }
		}

		public override string Usage
		{
			get { return "/spawn (int)NPC\n/spawn mod NPC"; }
		}

		public override string Description 
		{
			get { return ""; }
		}

		public override void Action(CommandCaller player, string input, string[] args)
		{
			int item;
			int pointless;
			int givenitem;
			int count = 1;
			if(args[0].ToLower()=="help"){
				string o = "";
				foreach(FieldInfo f in typeof(NPCID).GetFields()){
					if(f.Name.ToLower()!="count"){
						o+=f.Name+",";
						if(o.Length>60){
							Main.NewText(o);
							o = "";
						}
					}
				}
				if(o.Length>0)Main.NewText(o);
				Main.NewText("Count:"+NPCID.Count);
				return;
			}else if(int.TryParse(args[0], out pointless)){
				if(!int.TryParse(args[0], out item))return;
				if(args.Length == 2)int.TryParse(args[1], out count);
				givenitem = NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, int.Parse(args[0]));
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
			}else if(args.Length==1){
				Type type = typeof(NPCID);
				item = 0;
				if(type.GetField(args[0]) == null){
					ErrorMessage();
					return;
				}
				try{
					if(!int.TryParse(type.GetField(args[0]).GetRawConstantValue().ToString(),out item))ErrorMessage();
				}catch(NullReferenceException){
					ErrorMessage();
					return;
				}
				if(args.Length == 2)int.TryParse(args[1], out count);
				givenitem = NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
			}else{
				Mod itemmod = ModLoader.GetMod(args[0]);
				if(itemmod==null)return;
				if(itemmod.GetNPC(args[1])==null)return;
				item = itemmod.NPCType(args[1]);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
				if(count>1)for(int i = 1; i<count; i++)NPC.NewNPC((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, item);
				//givenitem = Item.NewItem(player.Player.Center, new Vector2(), item, count, false, 0, true);
			}
			Main.NewText("Player "+player.Player.name+" has successfully spawned "+Main.npc[givenitem].GivenOrTypeName+(count==1?"":"x"+count));
			//Main.NewText("Player "+player.Player.name+" was successfully given "+Main.item[givenitem].HoverName+" x"+count+"  [i/s"+count+":"+item+"]");
		}
		void ErrorMessage(){
			Main.NewText("Failed to spawn NPC. reason: invalid ID.", Color.OrangeRed);
			Main.NewText("Try \"/spawn help\" for a list of all NPC IDs.", Color.Orange);
		}
	}
	public class DebugGiveCommand : ModCommand{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}
		public override string Command
		{
			get { return "give"; }
		}

		public override string Usage
		{
			get { return "/give [vanilla|null|terraria] item count\n/give item count\n/give mod item count"; }
		}

		public override string Description 
		{
			get { return ""; }
		}

		public override void Action(CommandCaller player, string input, string[] args)
		{
			int item;
			int pointless;
			int givenitem;
			int count = 1;
			if(args[0].ToLower()=="help"){
				string o = "";
				foreach(FieldInfo f in typeof(ItemID).GetFields()){
					//if(f.Name.ToLower()!="count")Main.NewText(f.Name);
					if(f.Name.ToLower()!="count"){
						o+=f.Name+",";
						if(o.Length>60){
							Main.NewText(o);
							o = "";
						}
					}
				}
				if(o.Length>0)Main.NewText(o);
				Main.NewText("Count:"+ItemID.Count);
				return;
			}else if(args[0]==""){
				Type type = typeof(ItemID);
				item = 0;
				if(!int.TryParse(type.GetField(args[1]).GetRawConstantValue().ToString(),out item))ErrorMessage();//Main.NewText("Failed to give"+player.Player.name+" item. reason: invalid ID.", Color.OrangeRed);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = Item.NewItem(player.Player.Center, new Vector2(), item, count, false, 0, true);
			}else if(int.TryParse(args[0], out pointless)){
				if(!int.TryParse(args[0], out item))return;
				if(args.Length == 2)int.TryParse(args[1], out count);
            	givenitem = Item.NewItem(player.Player.Center, new Vector2(), int.Parse(args[0]), count, false, 0, true);
			}else{
				Mod itemmod = ModLoader.GetMod(args[0]);
				if(itemmod==null)return;
				if(itemmod.GetItem(args[1])==null)return;
				item = itemmod.ItemType(args[1]);
				if(args.Length == 3)int.TryParse(args[2], out count);
				givenitem = Item.NewItem(player.Player.Center, new Vector2(), item, count, false, 0, true);
			}
			Main.NewText("Player "+player.Player.name+" was successfully given "+Main.item[givenitem].HoverName+" x"+count+"  [i/s"+count+":"+item+"]");
		}
		void ErrorMessage(){
			Main.NewText("Failed to spawn Item. reason: invalid ID.", Color.OrangeRed);
			Main.NewText("Try \"/spawn help\" for a list of all Item IDs.", Color.Orange);
		}
	}
}