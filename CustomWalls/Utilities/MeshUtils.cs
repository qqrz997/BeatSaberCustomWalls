using System;
using System.Linq;
using UnityEngine;

namespace CustomWalls.Utilities;

internal static class MeshUtils
{
    public static MeshFilter GetMeshFilterByName(GameObject gameObject, string name)
    {
        return gameObject == null ? null : gameObject
            .GetGameObjectMeshFilter()
            .FirstOrDefault(filter => string.Equals(filter.name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static MeshFilter[] GetGameObjectMeshFilter(this GameObject gameObject, bool includeInactive = false)
    {
        return gameObject == null ? [] 
            : gameObject.GetComponentsInChildren<MeshFilter>(includeInactive) ?? [];
    }

    /// <summary>
    /// Creates a brand-new object that overlays on top of the normal wall
    /// </summary>
    public static GameObject CreateOverlay(Renderer target, Renderer donor, float offset)
    {
        var overlayObject = new GameObject("Overlay");
        overlayObject.transform.SetParent(target.transform);
        overlayObject.transform.localScale = new(
            1 + offset * (1 / target.transform.lossyScale.x),
            1 + offset * (1 / target.transform.lossyScale.y),
            1 + offset * (1 / target.transform.lossyScale.z)
        );
        overlayObject.transform.localPosition = new(0, 0, 0);
        overlayObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        var overlayRenderer = overlayObject.AddComponent<MeshRenderer>();
        overlayRenderer.material = donor.material;

        var overlayFilter = overlayObject.AddComponent<MeshFilter>();
        overlayFilter.mesh = target.gameObject.GetComponent<MeshFilter>().mesh;

        return overlayObject;
    }

    /// <summary>
    /// Copy over the essential parts of the donor over to the target mesh filter
    /// </summary>
    public static void ReplaceMesh(this MeshFilter target, MeshFilter donor, float scale)
    {
        target.mesh = donor.mesh;
        target.gameObject.transform.localScale *= scale;
    }
}