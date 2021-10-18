using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;

namespace DevHelp
{
	class DevHelp : Mod
	{
        internal static Mod mod;
        private HotKey ReadTooltipsVar = new HotKey("Toggle Advanced Tooltips", Keys.L);
		public DevHelp()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}
        
        public override void Load()
        {
            mod = this;
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };

            RegisterHotKey(ReadTooltipsVar.Name, ReadTooltipsVar.DefaultKey.ToString());
        }

        public override void HotKeyPressed(string name) {
            if(PlayerInput.Triggers.JustPressed.KeyStatus[GetTriggerName(name)]) {
                if(name.Equals(ReadTooltipsVar.Name)) {
                    ReadTooltips();
                }
            }
        }

        public void ReadTooltips()
        {
            Player player = Main.player[Main.myPlayer];
            DevPlayer modPlayer = player.GetModPlayer<DevPlayer>();
            modPlayer.readtooltips = !modPlayer.readtooltips;
            Main.PlaySound(12, player.Center);
        }

        public string GetTriggerName(string name)
        {
            return Name + ": " + name;
        }
	}
}
