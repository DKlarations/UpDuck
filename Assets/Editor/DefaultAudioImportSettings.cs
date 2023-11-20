using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DefaultAudioImportSettings : AssetPostprocessor 
{
    void OnPreprocessAudio()
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;

        // Adjust settings for specific platforms
        AudioImporterSampleSettings settings = new AudioImporterSampleSettings()
        {
            // Modify other settings as needed
            loadType = AudioClipLoadType.DecompressOnLoad,
            // ... set other properties as needed
        };

        // Apply settings for each desired platform
        audioImporter.SetOverrideSampleSettings("Standalone", settings);
        audioImporter.SetOverrideSampleSettings("WebGL", settings);

        // ... repeat for other platforms as needed

        audioImporter.loadInBackground = true; // Load audio in background
    }
}


