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
using DevHelp.UI;
using ReLogic.Content;
using Terraria.Audio;
using System.Reflection;
using System.Threading.Tasks;
using System;
using Terraria.GameContent;
using Terraria.UI.Chat;
using System.IO;
using System.Diagnostics;
using System.Reflection.Emit;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoOpCodes = Mono.Cecil.Cil.OpCodes;
using OpCodes = System.Reflection.Emit.OpCodes;
using Mono.Cecil;
using System.Text;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace DevHelp {
	public class DevHelp : Mod {
		internal static DevHelp instance;
		public static ModKeybind AdvancedTooltipsHotkey { get; private set; }
		public static ModKeybind RecipeMakerHotkey { get; private set; }
		public static ModKeybind RarityImageHotkey { get; private set; }

		public static bool readtooltips = false;

		public AutoCastingAsset<Texture2D>[] buttonTextures;

		internal static UserInterface UI;
		internal static RecipeMakerUI recipeMakerUI;
		internal static int customRecipeIndex;
		public static Recipe RecipeMakerRecipe => Main.recipe[customRecipeIndex];
		internal static Action RegenerateRequiredItemQuickLookup;
		public override void Load() {
			instance = this;
			if (Main.netMode != NetmodeID.Server) {
				UI = new UserInterface();

				buttonTextures = new AutoCastingAsset<Texture2D>[] {
					Assets.Request<Texture2D>("Checkbox"),
					Assets.Request<Texture2D>("Checkbox_Hovered"),
					Assets.Request<Texture2D>("Checkbox_Selected"),
					Assets.Request<Texture2D>("Checkbox_Selected_Hovered"),
					Assets.Request<Texture2D>("Button_Copy"),
					Assets.Request<Texture2D>("Button_Copy_Hovered")
				};
			}
			AdvancedTooltipsHotkey = KeybindLoader.RegisterKeybind(this, "Toggle Advanced Tooltips", Keys.L.ToString());
			RecipeMakerHotkey = KeybindLoader.RegisterKeybind(this, "Toggle Recipe Maker GUI", Keys.PageDown.ToString());
			RarityImageHotkey = KeybindLoader.RegisterKeybind(this, "Save Rarity Name Image", Keys.PrintScreen.ToString());
			DynamicMethodDefinition test = new(typeof(Recipe).GetMethod("CreateRequiredItemQuickLookups", BindingFlags.NonPublic | BindingFlags.Static));
			bool startPoint = false;
			bool firstLdLoc = false;
			bool secondLdLoc = false;
			///StringBuilder builder = new();
			for (int i = 0; i < test.Definition.Body.Instructions.Count; i++) {
				Instruction ins = test.Definition.Body.Instructions[i];
				///builder.Clear();
				///builder.Append($"{ins.Offset}: {ins.OpCode} {ins.Operand}");
				if (!startPoint) {
					if (ins.OpCode == MonoOpCodes.Ldsfld) {
						startPoint = true;
					} else {
						test.Definition.Body.Instructions.RemoveAt(i--);
						///builder.Append("; removed");
					}
				} else if (!firstLdLoc) {
					if (ins.OpCode == MonoOpCodes.Ldloc_0) {
						//TypeReference mod = new("DevHelp", nameof(DevHelp), new ModuleDefinition());
						//FieldReference field = new FieldReference(nameof(customRecipeIndex), typeof(int), typeof(DevHelp));
						FieldReference field = test.Module.ImportReference(typeof(DevHelp).GetField(nameof(customRecipeIndex), BindingFlags.NonPublic | BindingFlags.Static));
						test.Definition.Body.Instructions[i] = Instruction.Create(MonoOpCodes.Ldsfld, field);
						ins = test.Definition.Body.Instructions[i];
						///builder.Append($"; replaced with {ins.Offset}: {ins.OpCode} {ins.Operand}");
						firstLdLoc = true;
					}
				} else if (secondLdLoc || ins.OpCode == MonoOpCodes.Ldloc_0) {
					secondLdLoc = true;
					if (ins.OpCode != MonoOpCodes.Ret) {
						test.Definition.Body.Instructions.RemoveAt(i--);
						///builder.Append("; removed");
					}
				}
				///Logger.Info(builder.ToString());
			}
			RegenerateRequiredItemQuickLookup = test.Generate().CreateDelegate<Action>();
			On_CommonEnemyUICollectionInfoProvider.GetUnlockStateByKillCount_int_bool_int += (orig, killCount, quickUnlock, fullKillCountNeeded) => {
				if (DevHelpConfig.Instance.showFullBestiary) return BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return orig(killCount, quickUnlock, fullKillCountNeeded);
			};
			On_CritterUICollectionInfoProvider.GetEntryUICollectionInfo += (orig, self) => {
				BestiaryUICollectionInfo info = orig(self);
				if (DevHelpConfig.Instance.showFullBestiary) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return info;
			};
			On_GoldCritterUICollectionInfoProvider.GetEntryUICollectionInfo += (orig, self) => {
				BestiaryUICollectionInfo info = orig(self);
				if (DevHelpConfig.Instance.showFullBestiary) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return info;
			};
			On_HighestOfMultipleUICollectionInfoProvider.GetEntryUICollectionInfo += (orig, self) => {
				BestiaryUICollectionInfo info = orig(self);
				if (DevHelpConfig.Instance.showFullBestiary) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return info;
			};
			On_SalamanderShellyDadUICollectionInfoProvider.GetEntryUICollectionInfo += (orig, self) => {
				BestiaryUICollectionInfo info = orig(self);
				if (DevHelpConfig.Instance.showFullBestiary) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return info;
			};
			On_TownNPCUICollectionInfoProvider.GetEntryUICollectionInfo += (orig, self) => {
				BestiaryUICollectionInfo info = orig(self);
				if (DevHelpConfig.Instance.showFullBestiary) info.UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
				return info;
			};
		}

		public override void Unload() {
			instance = null;
			buttonTextures = null;
			AdvancedTooltipsHotkey = null;
			RecipeMakerHotkey = null;
			RarityImageHotkey = null;
			UI = null;
			recipeMakerUI = null;
			RecipeMakerUI.conditionFields = null;
		}
	}
	public class DevHelpConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static DevHelpConfig Instance;
		[DefaultValue(false)]
		public bool showFullBestiary;
	}
	public class DevSystem : ModSystem {
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.BoringBow);
			recipe.AddIngredient(ItemID.BoringBow);
			recipe.Register();
			DevHelp.customRecipeIndex = recipe.RecipeIndex;
		}
		public override void UpdateUI(GameTime gameTime) {
			DevHelp.UI?.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"DevHelp: RecipeMakerUI",
					delegate {
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						DevHelp.UI.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
	public class DevPlayer : ModPlayer {
		public static bool controlAdvancedTooltips;
		public static bool releaseAdvancedTooltips;
		public static bool controlRecipeMaker;
		public static bool releaseRecipeMaker;
		public override void ProcessTriggers(TriggersSet triggersSet) {
			bool tick = false;

			//releaseAdvancedTooltips = !controlAdvancedTooltips;
			//controlAdvancedTooltips = triggersSet.KeyStatus["DevHelp: Toggle Advanced Tooltips"];
			if (DevHelp.AdvancedTooltipsHotkey.JustPressed) {
				DevHelp.readtooltips = !DevHelp.readtooltips;
				tick = true;
			}

			//releaseRecipeMaker = !controlRecipeMaker;
			//controlRecipeMaker = triggersSet.KeyStatus["DevHelp: Toggle Recipe Maker GUI"];
			if (DevHelp.RecipeMakerHotkey.JustPressed) {
				if (DevHelp.recipeMakerUI is null) {
					DevHelp.recipeMakerUI = new RecipeMakerUI();
					DevHelp.recipeMakerUI.Activate();
					DevHelp.UI.SetState(DevHelp.recipeMakerUI);
				} else {
					DevHelp.UI.SetState(DevHelp.recipeMakerUI = null);
				}
				tick = true;
			}

			if (DevHelp.RarityImageHotkey.JustPressed) {
				Item item = ((Main.HoverItem?.IsAir) ?? true) ? Main.LocalPlayer.HeldItem : Main.HoverItem;
				int rare = item.rare;
				Type itemModType = item.ModItem?.GetType();
				//Task.Run(()=> {
				Vector2 size = default;
				string text = null;
				Color color = default;
				if (itemModType?.GetProperty("RarityName") is PropertyInfo uniqueRarityName) {
					string name = (string)uniqueRarityName.GetValue(null);
					size = FontAssets.MouseText.Value.MeasureString(name);
					text = name;
					color = Color.White;
					if (itemModType.GetMethod("GetCustomRarityDraw") is MethodInfo customDraw) {
						int frameNumber = 0;
						foreach (var frame in ((IEnumerable<(TextSnippet[] snippets, Vector2 offset, Color color)>)customDraw.Invoke(null, new object[] { text }))) {
							RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, (int)size.X + 8, (int)size.Y + 8);
							SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
							renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
							renderTarget.GraphicsDevice.Clear(Color.Transparent);
							spriteBatch.Begin();
							SpriteBatch realMainSB = Main.spriteBatch;
							try {
								Main.spriteBatch = spriteBatch;
								DrawableTooltipLine tooltipLine = new(
									new TooltipLine(Mod, "ItemName", text),
									0,
									4,
									4,
									color
								);
								ChatManager.DrawColorCodedStringWithShadow(
									Main.spriteBatch,
									tooltipLine.Font,
									frame.snippets,
									new Vector2(4, 4) + frame.offset,
									tooltipLine.Rotation,
									item.color,
									tooltipLine.Origin,
									tooltipLine.BaseScale,
									out _,
									tooltipLine.MaxWidth,
									tooltipLine.Spread
								);
							} finally {
								Main.spriteBatch = realMainSB;
							}
							spriteBatch.End();
							string folderPath = Path.Combine(Main.SavePath, "DevHelp", "Animated");
							if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
							string filePath = Path.Combine(folderPath, "Rare" + text + (frameNumber++)) + ".png";
							Stream stream = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);
							renderTarget.SaveAsPng(stream, (int)size.X + 8, (int)size.Y + 8);
							renderTarget.GraphicsDevice.SetRenderTarget(null);
						}
						text = null;
					}
				} else if (RarityLoader.GetRarity(rare) is ModRarity rarity) {
					if (rarity.GetType().GetProperty("RarityName") is PropertyInfo propertyInfo) {
						text = (string)propertyInfo.GetValue(null);
					} else {
						text = rarity.Name;
					}
					size = FontAssets.MouseText.Value.MeasureString(text);
					Main.mouseTextColor = 255;
					color = rarity.RarityColor;
					if (rarity.GetType().GetProperty("RarityAnimationFrames") is PropertyInfo animationInfo) {
						int max = (int)(byte)animationInfo.GetValue(null);
						for (int i = -max; i < max; i++) {
							Main.mouseTextColor = (byte)(190 + (65 * Math.Abs(i)) / max);
							color = rarity.RarityColor;
							RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, (int)size.X + 8, (int)size.Y + 8);
							SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
							renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
							renderTarget.GraphicsDevice.Clear(Color.Transparent);
							spriteBatch.Begin();
							SpriteBatch realMainSB = Main.spriteBatch;
							try {
								Main.spriteBatch = spriteBatch;
								int yoff = 0;
								DrawableTooltipLine tooltipLine = new(
									new TooltipLine(Mod, "ItemName", text),
									0,
									4,
									4,
									color
								);
								if (ItemLoader.PreDrawTooltipLine(Main.HoverItem, tooltipLine, ref yoff)) {
									ChatManager.DrawColorCodedStringWithShadow(
										spriteBatch,
										tooltipLine.Font,
										tooltipLine.Text,
										new Vector2(tooltipLine.X, tooltipLine.Y),
										tooltipLine.Color,
										tooltipLine.Rotation,
										tooltipLine.Origin,
										tooltipLine.BaseScale,
										tooltipLine.MaxWidth,
										tooltipLine.Spread
									);
								}
							} finally {
								Main.spriteBatch = realMainSB;
							}
							spriteBatch.End();
							string folderPath = Path.Combine(Main.SavePath, "DevHelp", "Animated");
							if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
							string filePath = Path.Combine(folderPath, "Rare" + text + (i + max)) + ".png";
							Stream stream = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);
							renderTarget.SaveAsPng(stream, (int)size.X + 8, (int)size.Y + 8);
							renderTarget.GraphicsDevice.SetRenderTarget(null);
						}
						text = null;
					}
				} else if(ItemRarityID.Search.TryGetName(rare, out string name)) {
					size = FontAssets.MouseText.Value.MeasureString(name);
					text = name;
					color = ItemRarity.GetColor(rare);
				}
				if (text is not null) {
					//GraphicsDevice graphicsDevice = new(Main.graphics.GraphicsDevice.Adapter, Main.graphics.GraphicsProfile, new PresentationParameters() {
					//});
					RenderTarget2D renderTarget = new(Main.graphics.GraphicsDevice, (int)size.X + 8, (int)size.Y + 8);
					SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
					renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
					renderTarget.GraphicsDevice.Clear(Color.Transparent);
					spriteBatch.Begin();
					SpriteBatch realMainSB = Main.spriteBatch;
					try {
						Main.spriteBatch = spriteBatch;
						int yoff = 0;
						DrawableTooltipLine tooltipLine = new DrawableTooltipLine(
							new TooltipLine(Mod, "ItemName", text),
							0,
							4,
							4,
							color
						);
						if (ItemLoader.PreDrawTooltipLine(Main.HoverItem, tooltipLine, ref yoff)) {
							ChatManager.DrawColorCodedStringWithShadow(
								spriteBatch,
								tooltipLine.Font,
								tooltipLine.Text,
								new Vector2(tooltipLine.X, tooltipLine.Y),
								tooltipLine.Color,
								tooltipLine.Rotation,
								tooltipLine.Origin,
								tooltipLine.BaseScale,
								tooltipLine.MaxWidth,
								tooltipLine.Spread
							);
						}
					} finally {
						Main.spriteBatch = realMainSB;
					}
					spriteBatch.End();
					string folderPath = Path.Combine(Main.SavePath, "DevHelp");
					if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
					string filePath = Path.Combine(folderPath, "Rare" + text) + ".png";
					Stream stream = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);
					renderTarget.SaveAsPng(stream, (int)size.X + 8, (int)size.Y + 8);
					renderTarget.GraphicsDevice.SetRenderTarget(null);
					new Process() {
						StartInfo = new ProcessStartInfo("explorer", filePath)
					}.Start();
					//Filters.Scene.EndCapture(null, Main.screenTarget, Main.screenTargetSwap, Color.Transparent);
					//graphicsDevice.SetRenderTarget(null);
				}
				//});
				tick = true;
			}

			if (tick) {
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}
	public struct AutoCastingAsset<T> where T : class {
		public bool HasValue => asset is not null;
		public bool IsLoaded => asset?.IsLoaded ?? false;
		public T Value => asset.Value;

		readonly Asset<T> asset;
		AutoCastingAsset(Asset<T> asset) {
			this.asset = asset;
		}
		public static implicit operator AutoCastingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator T(AutoCastingAsset<T> asset) => asset.Value;
	}
	public class FastStaticFieldInfo<TParent, T> : FastStaticFieldInfo<T> {
		public FastStaticFieldInfo(string name, BindingFlags bindingFlags, bool init = false) : base(typeof(TParent), name, bindingFlags, init) { }
	}
	public class FastStaticFieldInfo<T> {
		public readonly FieldInfo field;
		Func<T> getter;
		Action<T> setter;
		public FastStaticFieldInfo(Type type, string name, BindingFlags bindingFlags, bool init = false) {
			field = type.GetField(name, bindingFlags | BindingFlags.Static);
			if (field is null) throw new InvalidOperationException($"No such static field {name} exists");
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public FastStaticFieldInfo(FieldInfo field, bool init = false) {
			if (!field.IsStatic) throw new InvalidOperationException($"field {field.Name} is not static");
			this.field = field;
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public T GetValue() {
			return (getter ??= CreateGetter())();
		}
		public void SetValue(T value) {
			(setter ??= CreateSetter())(value);
		}
		private Func<T> CreateGetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(T), new Type[] { }, true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldsfld, field);
			gen.Emit(OpCodes.Ret);

			return (Func<T>)getterMethod.CreateDelegate(typeof(Func<T>));
		}
		private Action<T> CreateSetter() {
			if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[] { typeof(T) }, true);
			ILGenerator gen = setterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Stsfld, field);
			gen.Emit(OpCodes.Ret);

			return (Action<T>)setterMethod.CreateDelegate(typeof(Action<T>));
		}
		public static explicit operator T(FastStaticFieldInfo<T> fastFieldInfo) {
			return fastFieldInfo.GetValue();
		}
	}
	public class UnsafeFastFieldInfo<T> {
		public readonly FieldInfo field;
		Func<object, T> getter;
		Action<object, T> setter;
		public UnsafeFastFieldInfo(Type type, string name, BindingFlags bindingFlags, bool init = false) {
			field = type.GetField(name, bindingFlags | BindingFlags.Instance);
			var ttttt = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field is null) throw new ArgumentException($"could not find {name} in type {type} with flags {bindingFlags}");
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public UnsafeFastFieldInfo(FieldInfo field, bool init = false) {
			this.field = field;
			if (init) {
				getter = CreateGetter();
				setter = CreateSetter();
			}
		}
		public T GetValue(object parent) {
			//if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(T), new Type[] { parent.GetType() }, true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Ret);

			return (T)getterMethod.Invoke(null, new object[] { parent });
			//return (getter ??= CreateGetter())(parent);
		}
		public void SetValue(object parent, T value) {
			(setter ??= CreateSetter())(parent, value);
		}
		private Func<object, T> CreateGetter() {
			//if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(T), new Type[] { typeof(object) }, true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld, field);
			gen.Emit(OpCodes.Ret);

			return (Func<object, T>)getterMethod.CreateDelegate(typeof(Func<object, T>));
		}
		private Action<object, T> CreateSetter() {
			//if (field.FieldType != typeof(T)) throw new InvalidOperationException($"type of {field.Name} does not match provided type {typeof(T)}");
			string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[] { typeof(object), typeof(T) }, true);
			ILGenerator gen = setterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stfld, field);
			gen.Emit(OpCodes.Ret);

			return (Action<object, T>)setterMethod.CreateDelegate(typeof(Action<object, T>));
		}
	}
	public static class DevExtensions {
		public static void Clear(this Recipe recipe) {
			recipe.requiredItem.Clear();
			recipe.requiredTile.Clear();
			recipe.Conditions.Clear();
			typeof(Recipe).GetProperty("ConsumeItemHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
			typeof(Recipe).GetProperty("OnCraftHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
		}
		public static void RegenerateRequiredItemQuickLookup(this Recipe recipe) {

		}
	}
}
