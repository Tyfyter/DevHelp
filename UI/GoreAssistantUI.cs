using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.UI;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Terraria.GameContent;
using PegasusLib;
using System.Text;
using System;
using Microsoft.Xna.Framework.Input;
using static Mono.CompilerServices.SymbolWriter.CodeBlockEntry;
using static System.Net.Mime.MediaTypeNames;
using Terraria.GameInput;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.ID;
using PegasusLib.Graphics;
using ReLogic.OS;

namespace DevHelp.UI {
	public class GoreAssistantUI : UIState {
		public const string spawn_gore_template =
"""
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2({1}, {2}).RotatedBy(NPC.rotation),
					Vector2.Zero,
					mod.GetGoreSlot({0})
				);
""";
		public override void OnInitialize() {
			OverrideSamplerState = SamplerState.PointClamp;
			//List<Mod> mods = [];
			List<Jukebox_Selection> npcSelections = [];
			List<Jukebox_Selection> goreSelections = [];

			Gore_Assistant_Element goreAssistant = new() {
				Left = StyleDimension.FromPixelsAndPercent(0, 0f),
				Top = StyleDimension.FromPercent(0f),
				Width = new(360 * -2, 1 - 0.06f),
				Height = StyleDimension.FromPercent(1f),
				HAlign = 0.5f
			};
			Append(goreAssistant);
			for (int i = 0; i < ModLoader.Mods.Length; i++) {
				if (ModLoader.Mods[i].GetFileNames()?.Any(n => n.StartsWith("Gores/") && n.EndsWith(".rawimg")) ?? false) {
					//mods.Add(ModLoader.Mods[i]);
					NPC_Selector_Element npcSelectorElement = new();
					Gore_Selector_Element goreSelectorElement = new();
					npcSelectorElement.OnSelectNPC += goreAssistant.SelectNPC;
					goreSelectorElement.OnSelectGore += goreAssistant.SelectGore;
					foreach (ModNPC npc in ModLoader.Mods[i].GetContent<ModNPC>()) {
						if (Main.BestiaryDB.FindEntryByNPCID(npc.Type) is BestiaryEntry entry) {
							if (entry.Info.FirstOrDefault(i => i is NPCPortraitInfoElement) is NPCPortraitInfoElement portrait) {
								npcSelectorElement.elements.Add((
									npc.Type,
									portrait.ProvideUIElement(new() { OwnerEntry = entry, UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4 })
								));
							}
						}
					}
					foreach (string item in ModLoader.Mods[i].GetFileNames()) {
						if (item.StartsWith("Gores/") && item.EndsWith(".rawimg")) {
							goreSelectorElement.elements.Add(new(ModLoader.Mods[i], item.Replace(".rawimg", "")));
						}
					}
					npcSelections.Add(new(ModLoader.Mods[i].DisplayName, npcSelectorElement));
					goreSelections.Add(new(ModLoader.Mods[i].DisplayName, goreSelectorElement));
				}
			}
			Jukebox_Selector_Element npcJukebox = new(npcSelections) {
				Left = StyleDimension.FromPercent(0.03f),
				Top = StyleDimension.FromPercent(0.1f),
				Width = StyleDimension.FromPixels(360),
				Height = StyleDimension.FromPercent(0.9f)
			};
			Jukebox_Selector_Element goreJukebox = new(goreSelections) {
				Left = StyleDimension.FromPixelsAndPercent(-360, 0.97f),
				Top = StyleDimension.FromPercent(0.1f),
				Width = StyleDimension.FromPixels(360),
				Height = StyleDimension.FromPercent(0.9f)
			};
			npcJukebox.OnSelect += s => {
				goreJukebox.Selection = s;
				goreAssistant.Reset();
			};
			goreJukebox.OnSelect += s => {
				npcJukebox.Selection = s;
				goreAssistant.Reset();
			};
			Append(npcJukebox);
			Append(goreJukebox);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			Main.LocalPlayer.mouseInterface = true;
			base.Draw(spriteBatch);
		}
	}
	public class Search_Element : UIElement, ITextInputContainer {
		public StringBuilder searchText = null;
		public string Search {
			get => Parent is ISearcher searcher ? searcher.Search : "";
			set {
				if (Parent is ISearcher searcher) searcher.Search = value;
			}
		}
		public string TextDisplay => focused ? searchText.ToString() : Search;
		public int CursorIndex { get; set; } = 0;
		public StringBuilder Text => searchText;
		public bool focused = false;
		public bool hovering = false;
		bool mouseLeftLast = false;
		bool mouseRightLast = false;
		bool mouseMiddleLast = false;
		public override void OnInitialize() {
			IgnoresMouseInteraction = false;
		}
		public override void LeftClick(UIMouseEvent evt) {
			if (TextDisplay is not null && evt.MousePosition.X - this.GetDimensions().ToRectangle().Right > -30) {
				RightClick(evt);
				return;
			}
			searchText ??= new();
			CursorIndex = searchText.Length;
			focused = true;
		}
		public override void RightClick(UIMouseEvent evt) {
			this.Clear();
			Search = null;
			focused = false;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			float targetWidth = TextDisplay is null ? 0 : 1;
			if (Width.Percent != targetWidth) {
				Width.Percent = targetWidth;
				Parent.Recalculate();
			}
			hovering = ContainsPoint(Main.MouseScreen);
			if (Main.hasFocus) {
				bool mouseButtonPressed = false;
				if (Main.mouseLeft && !mouseLeftLast) {
					mouseButtonPressed = true;
					if (hovering && !IsMouseHovering) LeftClick(new(this, Main.MouseScreen));
				} else if (Main.mouseRight && !mouseRightLast) {
					mouseButtonPressed = true;
					if (hovering && !IsMouseHovering) RightClick(new(this, Main.MouseScreen));
				} else if (Main.mouseMiddle && !mouseMiddleLast) {
					mouseButtonPressed = true;
				}
				if (mouseButtonPressed && !hovering) {
					focused = false;
				}
			}
			mouseLeftLast = Main.mouseLeft;
			mouseRightLast = Main.mouseRight;
			mouseMiddleLast = Main.mouseMiddle;
		}
		AutoLoadingAsset<Texture2D> cancelbutton = "Terraria/Images/UI/SearchCancel";
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (focused) {
				if (searchText is null) {
					focused = false;
				} else {
					this.ProcessInput(out _);
				}
			}
			CalculatedStyle dimensions = this.GetDimensions();
			Utils.DrawSettingsPanel(spriteBatch, dimensions.Position(), dimensions.Width, hovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);
			Rectangle area = dimensions.ToRectangle();
			area.Width = area.Height;
			Rectangle clearButton = area;
			area.Inflate(-4, -4);
			spriteBatch.Draw(
				TextureAssets.Cursors[2].Value,
				area,
				Color.White * (hovering ? 1 : 0.8f)
			);
			if (TextDisplay is not null) {
				area.X += (int)dimensions.Width - 30;
				clearButton.X += (int)dimensions.Width - 30;
				spriteBatch.Draw(
					cancelbutton,
					area,
					Color.AliceBlue * (clearButton.Contains(Main.MouseScreen.ToPoint()) ? 1 : 0.8f)
				);
				Vector2 size = FontAssets.MouseText.Value.MeasureString(TextDisplay);
				float scale = Math.Min((dimensions.Width - 60) / (size.X + 1), 1.2f);
				this.DrawInputContainerText(spriteBatch,
					dimensions.Position() + new Vector2(30, dimensions.Height * 0.5f - 22 * scale * 0.5f),
					FontAssets.MouseText.Value,
					Color.White,
					focused,
					scale,
					new(8, 2)
				);
			}
		}
		void ITextInputContainer.Reset() {
			focused = false;
			searchText.Clear();
			if (Search is not null) searchText.Append(Search);
		}
		void ITextInputContainer.Submit() {
			Search = searchText.ToString();
			if (string.IsNullOrEmpty(Search)) Search = null;
			focused = false;
		}
	}
	public interface ISearcher {
		public string Search { get; set; }
	}
	public class SelectorElement : UIElement, ISearcher {
		string search = "";
		protected int searchChangedLevel = 2;
		public string Search {
			get => search;
			set {
				searchChangedLevel = (value ?? "").Contains(search) ? 1 : 2;
				search = value ?? "";
			}
		}
		protected Search_Element searchBox;
		public override void OnInitialize() {
			searchBox = new();
			searchBox.Height.Pixels = 30;
		}
	}
	public class NPC_Selector_Element : SelectorElement {
		public List<(int npc, UIElement portrait)> elements = [];
		public override void Draw(SpriteBatch spriteBatch) {
			switch (searchChangedLevel) {
				case 2: {
					searchChangedLevel = 0;
					RemoveAllChildren();
					if (searchBox is null) Initialize();
					float height = 0;
					Append(searchBox);
					height += searchBox.Height.Pixels + 2;
					bool isSearching = !string.IsNullOrEmpty(Search);
					for (int i = 0; i < elements.Count; i++) {
						UIElement child = elements[i].portrait;
						if (isSearching && !Lang.GetNPCNameValue(elements[i].npc).Contains(Search, StringComparison.CurrentCultureIgnoreCase)) continue;
						child.Top.Pixels = height;
						Append(child);
						height += child.Height.Pixels + 2;
					}
					break;
				}
				case 1: {
					float heightOffset = 0;
					for (int i = 0; i < elements.Count; i++) {
						if (elements[i].portrait.Parent == this) {
							if (Lang.GetNPCNameValue(elements[i].npc).Contains(Search, StringComparison.CurrentCultureIgnoreCase)) {
								elements[i].portrait.Top.Pixels -= heightOffset;
							} else {
								heightOffset += elements[i].portrait.Height.Pixels + 2;
								RemoveChild(elements[i].portrait);
							}
						}
					}
					break;
				}
			}
			base.Draw(spriteBatch);
		}
		public override void LeftClick(UIMouseEvent evt) {
			if (evt.Target is UIBestiaryNPCEntryPortrait portrait) {
				for (int i = 0; i < elements.Count; i++) {
					if (elements[i].portrait == portrait.Parent) {
						OnSelectNPC(elements[i].npc);
						return;
					}
				}
			}
		}
		public event Action<int> OnSelectNPC;
	}
	public class Gore_Selector_Element : SelectorElement {
		public List<Gore_Selection_Element> elements = [];
		public override void Draw(SpriteBatch spriteBatch) {
			switch (searchChangedLevel) {
				case 2: {
					searchChangedLevel = 0;
					RemoveAllChildren();
					if (searchBox is null) Initialize();
					float height = 0;
					Append(searchBox);
					height += searchBox.Height.Pixels + 2;
					bool isSearching = !string.IsNullOrEmpty(Search);
					for (int i = 0; i < elements.Count; i++) {
						UIElement child = elements[i];
						child.Initialize();
						if (isSearching && !elements[i].path.Contains(Search, StringComparison.CurrentCultureIgnoreCase)) continue;
						child.Top.Pixels = height;
						Append(child);
						height += child.Height.Pixels + 2;
					}
					break;
				}
				case 1: {
					float heightOffset = 0;
					for (int i = 0; i < elements.Count; i++) {
						if (elements[i].Parent == this) {
							if (elements[i].path.Contains(Search, StringComparison.CurrentCultureIgnoreCase)) {
								elements[i].Top.Pixels -= heightOffset;
								elements[i].Recalculate();
							} else {
								heightOffset += elements[i].Height.Pixels + 2;
								RemoveChild(elements[i]);
							}
						}
					}
					break;
				}
			}
			base.Draw(spriteBatch);
		}
		public override void LeftClick(UIMouseEvent evt) {
			if (evt.Target is Gore_Selection_Element gore) {
				OnSelectGore(gore);
			}
		}
		public event Action<Gore_Selection_Element> OnSelectGore;
	}
	public class Jukebox_Selector_Element(List<Jukebox_Selection> selections) : UIElement {
		public List<Jukebox_Selection> Selections { get; } = selections;
		int selection = 0;
		bool recursionGuard = false;
		public int Selection {
			get => selection;
			set {
				int oldSelection = selection;
				selection = (value + Selections.Count) % Selections.Count;
				if (selection != oldSelection && !recursionGuard) {
					try {
						recursionGuard = true;
						OnSelect(selection);
					} finally {
						recursionGuard = false;
					}
				}
			}
		}
		public event Action<int> OnSelect;
		public override void Draw(SpriteBatch spriteBatch) {
			if (Selections[selection].Element.Parent != this) {
				RemoveAllChildren();
				UIElement child = Selections[selection].Element;
				child.Top.Pixels = 30 + 2;
				child.Width.Percent = 1;
				child.Height.Percent = 1;
				Append(child);
				RecalculateChildren();
			}
			base.Draw(spriteBatch);
			CalculatedStyle dimensions = this.GetDimensions();
			if (Main.MouseScreen.Between(dimensions.Position(), dimensions.ToRectangle().BottomRight())) Main.LocalPlayer.mouseInterface = true;
			Vector2 startPos = dimensions.Position();
			void DrawPanel(Vector2 position, float width, string text, bool color) {
				Vector2 center = new Vector2(width, TextureAssets.SettingsPanel.Value.Height) * 0.5f;
				Utils.DrawSettingsPanel(spriteBatch, position, width, color ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlueMouseOver);
				float scale = color ? 0.95f : 0.85f;
				ChatManager.DrawColorCodedString(spriteBatch,
					FontAssets.ItemStack.Value,
					text,
					position + center,
					Color.White,
					0,
					FontAssets.ItemStack.Value.MeasureString(text) * 0.5f * scale,
					Vector2.One * scale
				);
			}
			float third = dimensions.Width / 3f;
			int hovering = 0;
			if (Main.MouseScreen.Between(startPos, startPos + new Vector2(third - 4, 30))) hovering = -1;
			if (Main.MouseScreen.Between(startPos + (third * 2 + 4) * Vector2.UnitX, startPos + (third * 2 + 4) * Vector2.UnitX + new Vector2(third - 4, 30))) hovering = 1;
			DrawPanel(startPos, third - 4, Selections[(selection + Selections.Count - 1) % Selections.Count].Name, hovering == -1);
			DrawPanel(startPos + third * Vector2.UnitX, third, Selections[selection].Name, true);
			DrawPanel(startPos + (third * 2 + 4) * Vector2.UnitX, third - 4, Selections[(selection + 1) % Selections.Count].Name, hovering == 1);
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				Selection += hovering;
			}
		}
	}
	public class Gore_Selection_Element(Mod mod, string path) : UIElement {
		public readonly string path = path;
		public readonly Asset<Texture2D> texture = mod.Assets.Request<Texture2D>(path);
		public bool selected;
		public bool hovered;
		public override void OnInitialize() {
			texture.Wait();
			Width.Pixels = texture.Width();
			Height.Pixels = texture.Height();
		}
		public override void Update(GameTime gameTime) {
			hovered = this.IsMouseHovering;
		}
		public override void Draw(SpriteBatch spriteBatch) {
			Rectangle frame = GetDimensions().ToRectangle();
			if (selected || hovered) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					frame with { X = frame.X - 2, Width = 2 },
					UICommon.DefaultUIBlueMouseOver
				);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					frame with { Y = frame.Y - 2, Height = 2 },
					UICommon.DefaultUIBlueMouseOver
				);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					frame with { X = frame.Right, Width = 2 },
					UICommon.DefaultUIBlueMouseOver
				);
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					frame with { Y = frame.Bottom, Height = 2 },
					UICommon.DefaultUIBlueMouseOver
				);
			}
			spriteBatch.Draw(
				texture.Value,
				frame,
				Color.White
			);
		}
		public override string ToString() => path;
		public Gore_Selection_Element Clone() => new(mod, path);
	}
	public class Gore_Assistant_Element : UIElement {
		NPC selectedNPC = null;
		List<Gore_Selection_Element> gores = [];
		public void Reset() {
			selectedNPC = null;
		}
		public void SelectNPC(int npc) {
			selectedNPC = new();
			selectedNPC.SetDefaults(npc);
			selectedNPC.IsABestiaryIconDummy = true;
		}
		public void SelectGore(Gore_Selection_Element gore) {
			Gore_Selection_Element newGore = gore.Clone();
			newGore.Left.Pixels = 0;
			newGore.Top.Pixels = 0;
			newGore.Initialize();
			newGore.Recalculate();
			gores.Add(newGore);
		}
		Gore_Selection_Element selectedGore;
		bool dragging = false;
		Vector2 oldMousePos;
		UIImage trashButton;
		UIImage copyButton;
		UIImage hideNPCHover;
		public override void OnInitialize() {
			trashButton = new UIImage(TextureAssets.Trash) {
				HAlign = 0f,
				VAlign = 1f,
				Left = new(4, 0),
				Top = new(-4, 0)
			};
			trashButton.OnLeftClick += (_, _) => {
				gores.Remove(selectedGore);
				selectedGore = null;
			};
			Append(trashButton);

			Append(new UIImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEye")) {
				HAlign = 0f,
				VAlign = 0f,
				Left = new(4, 0),
				Top = new(4, 0)
			});
			hideNPCHover = new UIImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEyeBack")) {
				HAlign = 0f,
				VAlign = 0f,
				Left = new(4, 0),
				Top = new(4, 0)
			};
			Append(hideNPCHover);

			copyButton = new UIImage(TextureAssets.Camera[0]) {
				HAlign = 1f,
				VAlign = 1f,
				Left = new(-4, 0),
				Top = new(-4, 0)
			};
			copyButton.OnLeftClick += (_, _) => {
				StringBuilder text = new();
				for (int i = 0; i < gores.Count; i++) {
					Rectangle rect = gores[i].GetDimensions().ToRectangle();
					if (text.Length > 0) text.AppendLine();
					text.Append($"""
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2({rect.X + rect.Width / 2} * NPC.direction, {rect.Y + rect.Height / 2}).RotatedBy(NPC.rotation),
					Vector2.Zero,
					mod.GetGoreSlot("{gores[i].path}")
				);
"""					);
				}
				Platform.Get<IClipboard>().Value =
$$"""
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
{{text}}
			}
		}
""";
				
			};
			Append(copyButton);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			if (selectedNPC is not null) {
				SpriteBatchState state = spriteBatch.GetState();
				try {
					Matrix matrix = state.transformMatrix;
					matrix.Down *= 4;
					matrix.Left *= 4;
					Vector2 backBufferSize = new(Main.graphics.PreferredBackBufferWidth, Main.graphics.PreferredBackBufferHeight);
					Vector2 offset = backBufferSize / 2;
					matrix.Translation += new Vector3(offset, 0);
					spriteBatch.Restart(state, transformMatrix: matrix);
					selectedNPC.Center = Main.screenPosition;
					if (!hideNPCHover.IsMouseHovering) {
						Lighting.AddLight(selectedNPC.Center, 1, 1, 1);
						Main.instance.DrawNPCDirect(spriteBatch, selectedNPC, true, Main.screenPosition);
						Main.instance.DrawNPCDirect(spriteBatch, selectedNPC, false, Main.screenPosition);
					}
					Vector2 mousePos = (Vector2.Transform(Main.MouseScreen + (backBufferSize - Main.ScreenSize.ToVector2()) * 0.5f, Matrix.Invert(matrix)));
					Gore_Selection_Element lastHoveredGore = null;
					for (int i = 0; i < gores.Count; i++) {
						Gore_Selection_Element gore = gores[i];
						if (!dragging && gore.GetDimensions().ToRectangle().Contains(mousePos.ToPoint())) lastHoveredGore = gore;
						gore.Draw(spriteBatch);
						gore.hovered = false;
					}
					if (lastHoveredGore is not null) lastHoveredGore.hovered = true;
					if (Main.mouseLeft == Main.mouseLeftRelease) {
						if (Main.mouseLeft && !trashButton.IsMouseHovering) {
							if (selectedGore is not null) selectedGore.selected = false;
							selectedGore = lastHoveredGore;
							if (selectedGore is not null) {
								selectedGore.selected = true;
								dragging = true;
							}
						} else {
							dragging = false;
						}
					} else if (dragging && selectedGore is not null) {
						selectedGore.Left.Pixels += mousePos.X - oldMousePos.X;
						selectedGore.Top.Pixels += mousePos.Y - oldMousePos.Y;
						selectedGore.Recalculate();
					}
					oldMousePos = mousePos;
				} finally {
					spriteBatch.Restart(state);
				}

			}
		}
	}
	public record class Jukebox_Selection(string Name, UIElement Element);
}