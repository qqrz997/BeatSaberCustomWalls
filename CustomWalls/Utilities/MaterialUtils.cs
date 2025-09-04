using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomWalls.Utilities;

internal static class MaterialUtils
{
    private static readonly System.Random mixStrength = new System.Random();
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public static Renderer MixRenderers(Renderer original, Renderer custom)
    {
        return DateTime.Now.Month == 4 && DateTime.Now.Day == 1 ? mixStrength.Next(100) == 0 ? original : custom : custom;
    }

    /// <summary>
    /// Find a specific renderer within a GameObject
    /// </summary>
    /// <param name="gameObject">GameObject</param>
    /// <param name="rendererName">Renderer name</param>
    public static Renderer GetGameObjectRenderer(GameObject gameObject, string rendererName)
    {
        IEnumerable<Renderer> renderers = GetGameObjectRenderer(gameObject);
        foreach (Renderer renderer in renderers)
        {
            if (string.Equals(renderer.name, rendererName, StringComparison.InvariantCultureIgnoreCase))
            {
                return renderer;
            }
        }

        return null;
    }

    /// <summary>
    /// Find all renderers within a GameObject
    /// </summary>
    /// <param name="gameObject">GameObject</param>
    /// <param name="includeInactive">Include inactive renderers</param>
    public static IEnumerable<Renderer> GetGameObjectRenderer(GameObject gameObject, bool includeInactive = false)
    {
        IEnumerable<Renderer> renderers = gameObject?.GetComponentsInChildren<Renderer>(includeInactive);
        return renderers ?? [];
    }

    /// <summary>
    /// Set the _Color field in every material if it has it
    /// </summary>
    /// <param name="materials">Materials</param>
    /// <param name="color">Color</param>
    public static void SetColors(this IEnumerable<Material> materials, Color color)
    {
        foreach (var material in materials)
        {
            if (material != null && material.HasProperty(ColorId))
            {
                material.SetColor(ColorId, color);
            }
        }
    }
}