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
		Func<int, ModRarity> _getRarity;
		Func<int, ModRarity> GetRarity => _getRarity ??= typeof(RarityLoader)
			.GetMethod("GetRarity", BindingFlags.NonPublic | BindingFlags.Static)
			.CreateDelegate<Func<int, ModRarity>>();
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
				int rare = Main.HoverItem.rare;
				Type itemModType = Main.HoverItem.ModItem?.GetType();
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
						foreach (var item in ((IEnumerable<(TextSnippet[] snippets, Vector2 offset, Color color)>)customDraw.Invoke(null, new object[] { text }))) {
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
									item.snippets,
									new Vector2(4, 4) + item.offset,
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
				} else if (GetRarity(rare) is ModRarity rarity) {
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
	public static class DevExtensions {
		public static void Clear(this Recipe recipe) {
			recipe.requiredItem.Clear();
			recipe.requiredTile.Clear();
			recipe.Conditions.Clear();
			typeof(Recipe).GetProperty("ConsumeItemHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
			typeof(Recipe).GetProperty("OnCraftHooks", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(recipe, null);
		}
	}
}
