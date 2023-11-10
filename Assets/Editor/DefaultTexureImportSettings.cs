using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DefaultTextureImportSettings : AssetPostprocessor 
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
