﻿namespace Obsidian.API.Crafting;

public sealed class SmeltingRecipe : IRecipe
{
    public required string Identifier { get; init; }

    public required CraftingType Type { get; init; }

    public string? Group { get; init; }

    public required Ingredient Result { get; init; }

    public required Ingredient Ingredient { get; init; }

    public required float Experience { get; init; }

    public required int CookingTime { get; init; }

    public required CookingBookCategory Category { get; init; }

    internal SmeltingRecipe() { }
}
