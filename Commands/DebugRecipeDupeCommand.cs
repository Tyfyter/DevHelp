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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DevHelp.Commands
{
	public class DebugRecipeDupeCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "recipeduplicates"; }
		}

		public override string Usage
		{
			get { return "/recipeduplicates"; }
		}

		public override string Description 
		{
			get { return "lists duplicate recipes"; }
		}
		//O(fuck) implementation
		public override void Action(CommandCaller player, string input, string[] args) {
			List<Recipe> firsts = new();
			List<Recipe> duplicates = new();
			void CheckRecipe(Recipe recipe) {
				if (firsts.Contains(recipe)) {
					duplicates.Add(recipe);
				} else {
					firsts.Add(recipe);
				}
			}
			for (int i = 0; i < Main.recipe.Length; i++) {
				Terraria.Recipe recipe = Main.recipe[i];
				if (!recipe.Disabled && !recipe.createItem.IsAir) {
					if (recipe.createItem.type == ItemID.Magiluminescence) {

					}
					List<Recipe.Item> ingredience = recipe.requiredItem.Select(i => new Recipe.Item(i.type, i.stack)).ToList();
					Recipe current = new() {
						output = new(recipe.createItem.type, recipe.createItem.stack),
						ingredients = ingredience.ToHashSet()
					};
					HashSet<Recipe> checks = new() {
						current
					};
					for (int j = 0; j < recipe.acceptedGroups.Count; j++) {
						RecipeGroup group = RecipeGroup.recipeGroups[recipe.acceptedGroups[j]];
						for (int k = 0; k < ingredience.Count; k++) {
							if (group.ContainsItem(ingredience[k].type)) {
								foreach (int item in group.ValidItems) {
									ingredience[k] = ingredience[k] with { type = item };
									current.ingredients = ingredience.ToHashSet();
									checks.Add(current);
								}
								break;
							}
						}
					}
					foreach (Recipe item in checks) CheckRecipe(item);
				}
			}
			if (duplicates.Any()) {
				foreach (Recipe item in duplicates) {
					player.Reply($"Duplicate recipe: {string.Join(", ", item.ingredients.Select(i => $"[i/s{i.count}:{i.type}]"))} --> [i/s{item.output.count}:{item.output.type}]");
				}
			} else {
				player.Reply("Did not find any duplicate recipes");
			}
		}
		internal struct Recipe {
			internal Item output;
			internal HashSet<Item> ingredients;
			internal record Item(int type, int count);
			public override bool Equals([NotNullWhen(true)] object obj) {
				return obj is Recipe other && this.output == other.output && this.ingredients.SetEquals(other.ingredients);
			}
			public override int GetHashCode() => output.GetHashCode();
		}
	}
}