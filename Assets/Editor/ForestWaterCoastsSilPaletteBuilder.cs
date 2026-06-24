using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ForestWaterCoastsSilPaletteBuilder
{
    private const string AnimatedTileFolder =
        "Assets/Spirtes/TailMapSource/Forest/Tiled_files/forest_water_tileSet/AnimatedTiles";

    private const string OutputFolder =
        "Assets/Spirtes/TailMapSource/Forest/Tiled_files/forest_water_coasts_sil_tileSet";

    private const string OutputPrefab =
        OutputFolder + "/forest_water_coasts_sil_tileSet_30x9.prefab";

    private const int Columns = 30;
    private const int TileCount = 270;

    [MenuItem("Tools/TileSets/Rebuild Water Coasts Sil Palette 30x9")]
    public static void RebuildPalette()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets/Spirtes/TailMapSource/Forest/Tiled_files",
                "forest_water_coasts_sil_tileSet");
        }

        var gridObject = new GameObject("forest_water_coasts_sil_tileSet_30x9");
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
                $"Water_coasts_sil_{index:0000}_anim.asset").Replace("\\", "/");

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

        Debug.Log($"Rebuilt Water_coasts_sil palette: {OutputPrefab}");
    }
}
