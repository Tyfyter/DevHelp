using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace DevHelp.UI
{
	// This class wraps the vanilla ItemSlot class into a UIElement. The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState.
	// By wrapping the vanilla ItemSlot class, we can easily use ItemSlot.
	// ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn.
	// If you want more control, you might need to write your own UIElement.
	// See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
    public class RefItemSlot : UIElement {
		internal Ref<Item> item;
		internal readonly int _context;
		internal readonly int color;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;
        protected internal int index = -1;
		public bool allowsRecipeGroups = false;
		public bool proxy = false;
		public RecipeGroup recipeGroup;
		public RefItemSlot(int colorContext = ItemSlot.Context.CraftingMaterial, int context = ItemSlot.Context.InventoryItem, float scale = 1f, Ref<Item> _item = null) {
			color = colorContext;
            _context = context;
			_scale = scale;
			item = _item??new Ref<Item>(new Item());
			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}
        protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();
			if (item is null) {
				item = new Ref<Item>(new Item());
            } else if (item.Value is null) {
				item.Value = new Item();
            }
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (proxy) {
					Main.hoverItemName = recipeGroup.GetText();
                    if(Main.mouseRight && Main.mouseRightRelease) {
                        foreach (KeyValuePair<int, RecipeGroup> entry in RecipeGroup.recipeGroups) {
                            if(recipeGroup is RecipeGroup){
                                if (recipeGroup == entry.Value) {
									recipeGroup = null;
									proxy = false;
                                }
								continue;
                            }
                            if (entry.Value.ContainsItem(item.Value.type)) {
								recipeGroup = entry.Value;
								proxy = true;
								goto skiphandle;
                            }
                        }
                    } else if (Main.mouseLeft && Main.mouseLeftRelease && Main.keyState.IsKeyDown(Main.FavoriteKey)) {
						recipeGroup = null;
						proxy = false;
                    }
				} else {
					if (allowsRecipeGroups && Main.mouseItem.IsAir) {
                        if (Main.mouseLeft && Main.mouseLeftRelease && Main.keyState.IsKeyDown(Main.FavoriteKey)) {
                            foreach (KeyValuePair<int, RecipeGroup> entry in RecipeGroup.recipeGroups) {
                                if (entry.Value.ContainsItem(item.Value.type)) {
									recipeGroup = entry.Value;
									proxy = true;
									goto skiphandle;
                                }
                            }
                        }
					}
					if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
						ItemSlot.Handle(ref item.Value, _context);
					}
				}
			}
			skiphandle:
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref item.Value, color, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}