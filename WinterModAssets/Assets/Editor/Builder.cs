using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Builder
{
    [MenuItem("Build/Build AssetBundles")]
    public static void BuildAssetbundles()
    {
        if (!Directory.Exists("Assets/AssetBundles"))
        {
            Directory.CreateDirectory("Assets/AssetBundles");
        }
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
