using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using System.Text;
using ReLogic.OS;
using Terraria.GameContent;
using System.Reflection;
using Terraria.ModLoader.Core;
using ReLogic.Content;
using System.Reflection.Emit;
using Terraria.GameContent.Bestiary;
using PegasusLib;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace DevHelp.UI {
	public class BiomeSelectorUI : UIState {
		static Dictionary<string, Action<Player>> biomeSetters = [];
		public override void OnInitialize() {
			int row = 0, col = 0;
			Action<Player> CreateBiomeSetter(string propertyName) {
				if (!biomeSetters.TryGetValue(propertyName, out Action<Player> setter)) {
					PropertyInfo property = typeof(Player).GetProperty(propertyName);
					string methodName = property.ReflectedType.FullName + ".set_" + property.Name;
					DynamicMethod setterMethod = new(methodName, null, [typeof(Player)], true);
					ILGenerator gen = setterMethod.GetILGenerator();

					gen.Emit(OpCodes.Ldarg_0);
					gen.Emit(OpCodes.Ldc_I4_1);
					gen.Emit(OpCodes.Call, property.SetMethod);
					gen.Emit(OpCodes.Ret);

					biomeSetters[propertyName] = setter = (Action<Player>)setterMethod.CreateDelegate(typeof(Action<Player>));
				}
				return setter;
			}
			Asset<Texture2D> iconTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Icon_Tags_Shadow");
			(string propertyName, SpawnConditionBestiaryInfoElement info)[] vanillaBiomes = [
				(nameof(Player.ZoneCorrupt), Biomes.TheCorruption),
				(nameof(Player.ZoneCrimson), Biomes.TheCrimson),
				(nameof(Player.ZoneHallow), Biomes.TheHallow),
				(nameof(Player.ZoneJungle), Biomes.Jungle),
				(nameof(Player.ZoneSnow), Biomes.Snow),
				(nameof(Player.ZoneDesert), Biomes.Desert),
				(nameof(Player.ZoneUndergroundDesert), Biomes.UndergroundDesert),
				(nameof(Player.ZoneSkyHeight), Biomes.Sky),
				(nameof(Player.ZoneOverworldHeight), Biomes.Surface),
				(nameof(Player.ZoneDirtLayerHeight), Biomes.Underground),
				(nameof(Player.ZoneRockLayerHeight), Biomes.Caverns),
				(nameof(Player.ZoneUnderworldHeight), Biomes.TheUnderworld),
				(nameof(Player.ZoneGranite), Biomes.Granite),
				(nameof(Player.ZoneMarble), Biomes.Marble),
				(nameof(Player.ZoneGraveyard), Biomes.Graveyard)
			];
			FastFieldInfo<FilterProviderInfoElement, Point> _filterIconFrame = new("_filterIconFrame", BindingFlags.NonPublic);
			for (int i = 0; i < vanillaBiomes.Length; i++) {
				(string propertyName, SpawnConditionBestiaryInfoElement info) = vanillaBiomes[i];
				Point frame = _filterIconFrame.GetValue(info);
				UIForcedVanillaBiomeButton iconButton = new(CreateBiomeSetter(propertyName), Language.GetText(info.GetDisplayNameKey()), iconTexture.Frame(16, 5, frame.X, frame.Y)) {
					Left = { Pixels = (float)(Main.screenWidth * 0.05 + 36 * col) },
					Top = { Pixels = (float)(Main.screenHeight * 0.4 + 36 * row) }
				};
				Append(iconButton);
				if (++col >= 10) {
					col = 0;
					row++;
				}
				
			}
			foreach (ModBiome biome in ModContent.GetContent<ModBiome>()) {
				if (!string.IsNullOrWhiteSpace(biome.BestiaryIcon) && ModContent.RequestIfExists<Texture2D>(biome.BestiaryIcon, out _)) {
					UIForcedBiomeButton iconButton = new(biome) {
						Left = { Pixels = (float)(Main.screenWidth * 0.05 + 36 * col) },
						Top = { Pixels = (float)(Main.screenHeight * 0.4 + 36 * row) }
					};
					Append(iconButton);
					if (++col >= 10) {
						col = 0;
						row++;
					}
				}
			}
		}
	}
	public class UIForcedVanillaBiomeButton(Action<Player> setActive, LocalizedText name, Rectangle frame) : UIIconButton(null, name) {
		bool isSelected;
		readonly Asset<Texture2D> iconTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/Bestiary/Icon_Tags_Shadow");
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			isSelected = DevHelp.forcedVanillaBiomes.Contains(setActive);
			SetCurrentOption(isSelected);
		}
		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			isSelected = DevHelp.forcedVanillaBiomes.Contains(setActive);
			if (isSelected) DevHelp.forcedVanillaBiomes.Remove(setActive);
			else DevHelp.forcedVanillaBiomes.Add(setActive);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			Color color2 = Color.White;
			if (!IsMouseHovering && !isSelected) {
				color2 = Color.Lerp(Colors.InventoryDefaultColor, Color.White, 0.7f) * 0.7f;
			}
			CalculatedStyle dimensions = GetDimensions();
			spriteBatch.Draw(iconTexture.Value, new Vector2(dimensions.X + 1f, dimensions.Y + 1f), frame, color2);
		}
	}
	public class UIForcedBiomeButton(ModBiome biome) : UIIconButton(ModContent.Request<Texture2D>(biome.BestiaryIcon), biome.DisplayName) {
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			SetCurrentOption(DevHelp.forcedModBiomes.Contains(biome));
		}
		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			if (!DevHelp.forcedModBiomes.Add(biome)) DevHelp.forcedModBiomes.Remove(biome);
		}
	}
}