using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DevHelp {
    internal class DevPlayer : ModPlayer {
		public bool readtooltips = false;
        public override bool Autoload(ref string name) {
            return true;
        }
    }
}
