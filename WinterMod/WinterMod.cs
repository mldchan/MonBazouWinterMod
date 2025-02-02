using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WinterMod
{
    [BepInPlugin("me.mldchan.wintermod", "Winter Mod", "1.0")]
    public class WinterMod : BaseUnityPlugin
    {
        private ConfigEntry<bool> enabled;

        private void Awake()
        {
            enabled = Config.Bind("General", "Enabled", true, "Enable the mod");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex != 2) return;

            if (!enabled.Value) return;

            Debug.Log("WinterMod Loading...");

            var ass = LoadAssetBundle();
            if (!ass)
            {
                Debug.LogError("Unable to load snow.unity3d, aborting....");
                return;
            }

            var material = ass.LoadAsset<Material>("Snow");

            ass.Unload(false);

            var terrains = GameObject.Find("/Terrains");

            // Mainland textures

            for (var i = 0; i < 4; i++)
                SwapTerrainTextures(terrains.transform.GetChild(i).GetComponent<Terrain>(), material);

            SwapTerrainTextures(terrains.transform.GetChild(4).GetComponent<Terrain>(), material, true);

            var mainlands = terrains.transform.GetChild(5);

            // Dealerland textures
            for (var i = 0; i < 4; i++)
                SwapTerrainTextures(mainlands.transform.GetChild(i).GetComponent<Terrain>(), material);

            ReplaceRoadTextures(terrains, material);
        }

        private static void ReplaceRoadTextures(GameObject terrains, Material material)
        {
            Debug.Log("Swapping road textures");
            // Replace road textures
            // 1. At home
            for (var i = 0; i < 6; i++)
                try
                {
                    terrains.transform.GetChild(0).GetChild(0).GetChild(i).GetComponent<Renderer>().material = material;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName} {e.Message} on line 67");
                }

            // 2. City
            // ReSharper disable once GrammarMistakeInComment
            // ReSharper disable once StringLiteralTypo
            var parent = GameObject.Find("/ENVIRONEMENT").transform.GetChild(1);
            for (var i = 0; i < parent.childCount; i++)
            {
                if (!parent.GetChild(i).name.Contains("Road")) continue;
                try
                {
                    parent.GetChild(i).GetComponent<Renderer>().material = material;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName} {e.Message} on line 81 index {i}");
                }
            }
            
            // Intersections City
            var intersections = parent.GetChild(0);
            for (int i = 0; i < intersections.childCount; i++)
            {
                try
                {
                    intersections.GetChild(i).GetChild(0).GetComponent<Renderer>().material = material;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName} {e.Message} on line 86 index {i}");
                }
            }

            // 3. Mainland
            parent = GameObject.Find("RoadNetwork_Mainland").transform.GetChild(1);
            for (var i = 0; i < 4; i++)
                try
                {
                    parent.GetChild(i).GetChild(0).GetComponent<Renderer>().material = material;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName} {e.Message} on line 94 index {i}");
                }

            // Racetrack Island
            var raceTrackIsland = GameObject.Find("/RaceTrackIsland");
            try
            {
                raceTrackIsland.transform.GetChild(1).GetChild(1).GetChild(0)
                    .GetComponent<Renderer>()
                    .material = material;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.GetType().FullName} {e.Message} on line 105");
            }

            // Swap roads on the racetrack island
            for (var i = 0; i < 7; i++)
                try
                {
                    raceTrackIsland.transform.GetChild(11).GetChild(0).GetChild(i).GetComponent<Renderer>().material = material;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName} {e.Message} on line 118 index {i}");
                }

            try
            {
                raceTrackIsland.transform.GetChild(11).GetChild(0).GetChild(24).GetComponent<Renderer>().material = material;
                raceTrackIsland.transform.GetChild(11).GetChild(0).GetChild(25).GetComponent<Renderer>().material = material;
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.GetType().FullName} {e.Message} on line 127");
            }
        }

        private AssetBundle LoadAssetBundle()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var str = thisAssembly.GetManifestResourceStream("WinterMod.Resources.snow.unity3d");

            var bytes = new byte[str.Length];
            _ = str.Read(bytes, 0, bytes.Length);
            str.Close();
            var assetBundle = AssetBundle.LoadFromMemory(bytes);

            return assetBundle;
        }

        private static void SwapTerrainTextures(Terrain t, Material m, bool earthLayer = false)
        {
            Debug.Log($"Swapping textures on {t.transform.parent.name}/{t.gameObject.name}...");

            for (var i = 0; i < 6; i++)
                try
                {
                    t.terrainData.terrainLayers[i].diffuseTexture = TextureToTexture2D(m.GetTexture("_MainTex"));
                    t.terrainData.terrainLayers[i].normalMapTexture = TextureToTexture2D(m.GetTexture("_BumpMap"));
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.GetType().FullName}: ${e.Message} on line 156");
                }

            if (!earthLayer) return;
            try
            {
                t.terrainData.terrainLayers[6].diffuseTexture = TextureToTexture2D(m.GetTexture("_MainTex"));
                t.terrainData.terrainLayers[6].normalMapTexture = TextureToTexture2D(m.GetTexture("_BumpMap"));
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.GetType().FullName}: ${e.Message} on line 167");
            }
        }

        private static Texture2D TextureToTexture2D(Texture t)
        {
            var texture2D = new Texture2D(t.width, t.height, TextureFormat.RGBA32, false);

            var currentRT = RenderTexture.active;

            var renderTexture = new RenderTexture(t.width, t.height, 32);
            Graphics.Blit(t, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;

            return texture2D;
        }
    }
}