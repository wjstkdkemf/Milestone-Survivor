using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ForestWaterDetilazationPaletteBuilder
{
    private const string AnimatedTileFolder =
        "Assets/Spirtes/TailMapSource/Forest/Tiled_files/forest_water_detilazation_tileSet/AnimatedTiles";

    private const string OutputFolder =
        "Assets/Spirtes/TailMapSource/Forest/Tiled_files/forest_water_detilazation_tileSet";

    private const string OutputPrefab =
        OutputFolder + "/forest_water_detilazation_tileSet_37x13.prefab";

    private const int Columns = 37;
    private const int TileCount = 481;

    [MenuItem("Tools/TileSets/Rebuild Water Detilazation Palette 37x13")]
    public static void RebuildPalette()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets/Spirtes/TailMapSource/Forest/Tiled_files",
                "forest_water_detilazation_tileSet");
        }

        var gridObject = new GameObject("forest_water_detilazation_tileSet_37x13");
        gridObject.AddComponent<Grid>();

        var layerObject = new GameObject("Layer1");
        layerObject.transform.SetParent(gridObject.transform, false);

        var tilemap = layerObject.AddComponent<Tilemap>();
        tilemap.tileAnchor = new Vector3(0.5f, 0f, 0f);
        layerObject.AddComponent<TilemapRenderer>();

        for (var index = 0; index < TileCount; index++)
        {
            var tilePath = Path.Combine(
                AnimatedTileFolder,
                $"water_detilazation_{index:0000}_anim.asset").Replace("\\", "/");

            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
            if (tile == null)
            {
                Debug.LogError($"Missing tile asset: {tilePath}");
                Object.DestroyImmediate(gridObject);
                return;
            }

            var position = new Vector3Int(index % Columns, -(index / Columns), 0);
            tilemap.SetTile(position, tile);
        }

        tilemap.RefreshAllTiles();
        PrefabUtility.SaveAsPrefabAsset(gridObject, OutputPrefab);
        Object.DestroyImmediate(gridObject);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Rebuilt water_detilazation palette: {OutputPrefab}");
    }
}
