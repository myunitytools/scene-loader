#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
#endif
using UnityEngine.TestTools;
using NUnit.Framework;
#if ENABLE_ADDRESSABLES
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif
using UnityEngine.AddressableAssets;
#endif

namespace MyGameDevTools.SceneLoading.Tests
{
    public class SceneTestEnvironment : IPrebuildSetup, IPostBuildCleanup
    {
        public const string ScenePathBase = "Assets/_test";
        public const int DefaultTimeout = 3000;

        static readonly ILoadSceneInfo[][] _multipleLoadSceneInfoList = new ILoadSceneInfo[][]
        {
            new ILoadSceneInfo[]
            {
                new LoadSceneInfoName(SceneBuilder.SceneNames[0]),
                new LoadSceneInfoIndex(1),
#if ENABLE_ADDRESSABLES
                new LoadSceneInfoAddress(SceneBuilder.SceneNames[2]),
                new LoadSceneInfoAddress(SceneBuilder.SceneNames[3]),
#endif
                new LoadSceneInfoName(SceneBuilder.ScenePaths[3])
            },
            // This list of scenes expects two load scene infos that point to the same source scene,
            // and validates whether that causes any issues when linking to the target loaded scene.
            new ILoadSceneInfo[]
            {
                new LoadSceneInfoIndex(1),
                new LoadSceneInfoName(SceneBuilder.SceneNames[1]),
                new LoadSceneInfoName(SceneBuilder.ScenePaths[1]),
#if ENABLE_ADDRESSABLES
                // Since we can't test statically with AssetReference, we should at least validate
                // that two AsyncOperations with the same addressable source do not cause issues.
                new LoadSceneInfoAddress(SceneBuilder.SceneNames[1]),
                new LoadSceneInfoAddress(SceneBuilder.SceneNames[1]),
#endif
            }
        };

        public static readonly ILoadSceneInfo[] SingleLoadSceneInfoList = new ILoadSceneInfo[]
        {
            new LoadSceneInfoName(SceneBuilder.SceneNames[1]),
            new LoadSceneInfoName(SceneBuilder.ScenePaths[1]),
            new LoadSceneInfoIndex(1),
#if ENABLE_ADDRESSABLES
            new LoadSceneInfoAddress(SceneBuilder.SceneNames[1]),
#endif
        };

        public static readonly ILoadSceneInfo[] SingleLoadSceneInfoList_NoAddressable = new ILoadSceneInfo[]
        {
            new LoadSceneInfoName(SceneBuilder.SceneNames[1]),
            new LoadSceneInfoName(SceneBuilder.ScenePaths[1]),
            new LoadSceneInfoIndex(1),
        };

        public static readonly SceneParameters[] SceneParametersList = new SceneParameters[]
        {
            new(SingleLoadSceneInfoList[0], false),
            new(SingleLoadSceneInfoList[0], true),
            new(SingleLoadSceneInfoList[1], false),
            new(SingleLoadSceneInfoList[1], true),
            new(SingleLoadSceneInfoList[2], false),
            new(SingleLoadSceneInfoList[2], true),
#if ENABLE_ADDRESSABLES
            new(SingleLoadSceneInfoList[3], false),
            new(SingleLoadSceneInfoList[3], true),
#endif
            new(_multipleLoadSceneInfoList[0], -1),
            new(_multipleLoadSceneInfoList[0], 1),
            new(_multipleLoadSceneInfoList[1], -1),
            new(_multipleLoadSceneInfoList[1], 1),
        };
        public static readonly SceneParameters[] TransitionSceneParametersList = new SceneParameters[]
        {
            new(SingleLoadSceneInfoList[0], true),
            new(SingleLoadSceneInfoList[1], true),
            new(SingleLoadSceneInfoList[2], true),
#if ENABLE_ADDRESSABLES
            new(SingleLoadSceneInfoList[3], true),
#endif
            new(_multipleLoadSceneInfoList[0], 1),
            new(_multipleLoadSceneInfoList[1], 1),
        };

        public static readonly ISceneManager[] SceneManagers = new ISceneManager[]
        {
            new AdvancedSceneManager(),
        };

#if UNITY_EDITOR
#if ENABLE_ADDRESSABLES
        const string _addressableScenePathBase = "Assets/_addressables-test";
        const string _sceneReferencePath = _addressableScenePathBase + "/sceneReference.asset";
#endif
#endif

        public void Setup()
        {
#if UNITY_EDITOR
            if (IsSceneEnvironmentSetup())
                return;

            int sceneCount = SceneBuilder.SceneNames.Length;
            List<EditorBuildSettingsScene> buildScenes = new(sceneCount);

            if (!SceneBuilder.TryBuildScenes(ScenePathBase, (i, s, p) => buildScenes.Add(new EditorBuildSettingsScene(p, true))))
                return;

            Debug.Log("Adding test scenes to build settings:\n" + string.Join("\n", buildScenes.Select(scene => scene.path)));
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Union(buildScenes).ToArray();

#if ENABLE_ADDRESSABLES
            SceneReferenceData sceneReferenceData = ScriptableObject.CreateInstance<SceneReferenceData>();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            SceneBuilder.TryBuildScenes(_addressableScenePathBase, (i, s, p) =>
            {
                string guid = AssetDatabase.AssetPathToGUID(p);
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.SetAddress(SceneBuilder.SceneNames[i]);

                sceneReferenceData.sceneReferences.Add(new AssetReference(guid));
            });

            AssetDatabase.CreateAsset(sceneReferenceData, _sceneReferencePath);
            var guid = AssetDatabase.AssetPathToGUID(_sceneReferencePath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.SetAddress(nameof(SceneReferenceData));
#endif
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            if (!IsSceneEnvironmentSetup())
                return;

            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene => !scene.path.StartsWith(ScenePathBase)).ToArray();

            if (!Directory.Exists(ScenePathBase))
                return;

            AssetDatabase.DeleteAsset(ScenePathBase);
            AssetDatabase.Refresh();

#if ENABLE_ADDRESSABLES
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var scenePaths = EditorBuildSettings.scenes.Where(scene => scene.path.StartsWith(_addressableScenePathBase)).Select(scene => scene.path);
            foreach (var path in scenePaths)
                settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(path), false);

            settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(_sceneReferencePath), false);

            AssetDatabase.DeleteAsset(_addressableScenePathBase);
            AssetDatabase.Refresh();
#endif
#endif
        }

        public static void ValidateSceneEnvironment()
        {
#if UNITY_EDITOR
            Assert.True(IsSceneEnvironmentSetup());

#if ENABLE_ADDRESSABLES
            var operation = Addressables.LoadResourceLocationsAsync(SceneBuilder.SceneNames);
            operation.WaitForCompletion();

            Assert.True(areLocationsValid());

            bool areLocationsValid()
            {
                foreach (var location in operation.Result)
                    if (location == null || string.IsNullOrEmpty(location.PrimaryKey))
                        return false;
                return true;
            }
#endif
#endif
        }

        public static bool IsSceneEnvironmentSetup()
        {
#if UNITY_EDITOR
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            foreach (string name in SceneBuilder.SceneNames)
            {
                if (!hasBuiltSceneWithName(name, buildScenes))
                    return false;
            }
            return true;

            static bool hasBuiltSceneWithName(string name, EditorBuildSettingsScene[] buildScenes)
            {
                foreach (EditorBuildSettingsScene buildScene in buildScenes)
                    if (buildScene.path.Contains(name))
                        return true;
                return false;
            }
#else
            return false;
#endif
        }
    }
}