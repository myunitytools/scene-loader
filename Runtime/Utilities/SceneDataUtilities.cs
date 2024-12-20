using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace MyGameDevTools.SceneLoading
{
    /// <summary>
    /// Static utility class to handle common <see cref="ISceneData"/> operations.
    /// </summary>
    public static class SceneDataUtilities
    {
        /// <summary>
        /// Links an array of <see cref="ISceneData"/> to the last loaded scenes, excluding already linked <see cref="ISceneData"/> via the parameter <paramref name="sceneDatasToExclude"/>.
        /// <br/>
        /// First, the <paramref name="sceneDatasToExclude"/> removes scenes already linked from the possible match list.
        /// Then, the <see cref="ISceneData"/> that have a direct reference to their loaded scenes (in <see cref="IAsyncSceneOperation.HasDirectReferenceToScene"/>) are linked.
        /// Finally, the remaining <see cref="ISceneData"/> are linked through indirect reference via their <see cref="ILoadSceneInfo"/> (by <see cref="ILoadSceneInfo.IsReferenceToScene(Scene)"/>).
        /// </summary>
        public static void LinkLoadedScenesWithSceneDataArray(ISceneData[] sceneDataArray, IList<ISceneData> sceneDatasToExclude)
        {
            // Fill this list with all loaded scenes from the Unity Scene Manager;
            int totalSceneCount = UnitySceneManager.sceneCount;
            List<Scene> unmatchedScenes = new(totalSceneCount);

            int i;
            for (i = 0; i < totalSceneCount; i++)
            {
                Scene scene = UnitySceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                    unmatchedScenes.Add(scene);
            }

            // Remove scenes already tracked by the scene manager
            totalSceneCount = sceneDatasToExclude.Count;
            for (i = 0; i < totalSceneCount; i++)
            {
                unmatchedScenes.Remove(sceneDatasToExclude[i].SceneReference);
            }

            // Loop through all ISceneData and update those that have direct reference to their scene
            // through their ILoadSceneOperation.
            int sceneDataCount = sceneDataArray.Length;
            List<ISceneData> unmatchedSceneDatas = new(sceneDataArray);

            ISceneData sceneData;
            for (i = 0; i < sceneDataCount; i++)
            {
                sceneData = sceneDataArray[i];
                if (sceneData.AsyncOperation.HasDirectReferenceToScene)
                {
                    sceneData.UpdateSceneReference();
                    unmatchedScenes.Remove(sceneData.SceneReference);
                    unmatchedSceneDatas.Remove(sceneData);
                }
            }

            // Then, loop through all remaining unmatched scenes and check if they can match one of the indirect reference loaded scenes.
            for (i = unmatchedScenes.Count - 1; i >= 0 && unmatchedSceneDatas.Count > 0; i--)
            {
                if (TryLinkLoadedSceneWithSceneData(unmatchedScenes[i], unmatchedSceneDatas, out ISceneData matchedData))
                {
                    matchedData.SetSceneReferenceManually(unmatchedScenes[i]);
                    unmatchedScenes.RemoveAt(i);
                    unmatchedSceneDatas.Remove(matchedData);
                }
            }

            if (unmatchedSceneDatas.Count > 0)
            {
                throw new Exception($"Unable to link all scene datas to loaded scenes. Linked {sceneDataCount - unmatchedSceneDatas.Count}/{sceneDataCount}.");
            }
        }

        /// <summary>
        /// Gets an array of <see cref="ISceneData"/>, from a list of loaded <see cref="ISceneData"/>, that have been loaded through the given array of <see cref="ILoadSceneInfo"/>.
        /// </summary>
        public static ISceneData[] GetLoadedSceneDatasWithLoadSceneInfos(ILoadSceneInfo[] sourceSceneInfos, IList<ISceneData> loadedSceneDataList)
        {
            int sceneCount = sourceSceneInfos.Length;
            ISceneData[] sceneDataArray = new ISceneData[sceneCount];

            List<ISceneData> unmatchedSceneDatas = new(loadedSceneDataList);
            for (int i = sceneCount - 1; i >= 0; i--)
            {
                if (TryGetSceneDataByLoadSceneInfo(sourceSceneInfos[i], unmatchedSceneDatas, out ISceneData matchedSceneData))
                {
                    sceneDataArray[i] = matchedSceneData;
                    unmatchedSceneDatas.Remove(matchedSceneData);
                }
                else
                {
                    throw new Exception($"Unable to match scene data with load scene info {sourceSceneInfos[i]}");
                }
            }

            return sceneDataArray;
        }

        /// <summary>
        /// Gets an array of scenes loaded by the given <see cref="ISceneData"/> array.
        /// </summary>
        public static Scene[] GetScenesFromSceneDataArray(ISceneData[] sceneDataArray)
        {
            int sceneCount = sceneDataArray.Length;
            Scene[] loadedScenes = new Scene[sceneCount];
            for (int i = 0; i < sceneCount; i++)
            {
                loadedScenes[i] = sceneDataArray[i].SceneReference;
            }
            return loadedScenes;
        }

        /// <summary>
        /// Gets the average progress of an array of <see cref="ISceneData"/>'s <see cref="IAsyncSceneOperation"/>s.
        /// </summary>
        public static float GetAverageSceneLoadOperationProgress(ISceneData[] sceneDataArray)
        {
            int length = sceneDataArray.Length;
            float totalProgress = 0;
            for (int i = 0; i < length; i++)
            {
                totalProgress += sceneDataArray[i].AsyncOperation.Progress;
            }
            return totalProgress / length;
        }

        /// <summary>
        /// Attempts to link a loaded scene with an <see cref="ISceneData"/>, that cannot directly link to its loaded scene (has a false <see cref="IAsyncSceneOperation.HasDirectReferenceToScene"/>), from a list.
        /// </summary>
        /// <param name="scene">The loaded scene to be linked.</param>
        /// <param name="sceneDataList">The list of <see cref="ISceneData"/> to validate.</param>
        /// <param name="matchedData">The matched <see cref="ISceneData"/>.</param>
        /// <returns>Whether a match has been found</returns>
        public static bool TryLinkLoadedSceneWithSceneData(Scene scene, IList<ISceneData> sceneDataList, out ISceneData matchedData)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException($"Cannot try to link an invalid or unloaded scene: {scene.name} ({scene.handle}).", nameof(scene));
            }

            int sceneDataCount = sceneDataList.Count;
            for (int i = sceneDataCount - 1; i >= 0; i--)
            {
                if (sceneDataList[i].LoadSceneInfo.CanBeReferenceToScene(scene))
                {
                    matchedData = sceneDataList[i];
                    return true;
                }
            }

            Debug.LogWarning($"Unable to link the loaded scene {scene.name} ({scene.handle}) with any of the provided {nameof(ISceneData)}.");
            matchedData = null;
            return false;
        }

        /// <summary>
        /// Tries to get an <see cref="ISceneData"/> from a collection of <see cref="ISceneData"/>s that match the given <see cref="ILoadSceneInfo"/>.
        /// </summary>
        public static bool TryGetSceneDataByLoadSceneInfo(ILoadSceneInfo loadSceneInfo, ICollection<ISceneData> sceneDataList, out ISceneData sceneData)
        {
            if (loadSceneInfo == null)
                throw new ArgumentNullException(nameof(loadSceneInfo));

            foreach (ISceneData tempSceneData in sceneDataList)
            {
                if ((loadSceneInfo.Type == LoadSceneInfoType.SceneHandle && tempSceneData.SceneReference == (Scene)loadSceneInfo.Reference)
                    || tempSceneData.LoadSceneInfo.Equals(loadSceneInfo))
                {
                    sceneData = tempSceneData;
                    return true;
                }
            }

            Debug.LogWarning($"Unable to get an {nameof(ISceneData)} with the load scene info {loadSceneInfo}.");
            sceneData = default;
            return false;
        }

        /// <summary>
        /// Tries to get an <see cref="ISceneData"/> from a collection of <see cref="ISceneData"/>s that match the given loaded <see cref="Scene"/>.
        /// </summary>
        public static bool TryGetSceneDataByLoadedScene(Scene scene, ICollection<ISceneData> sceneDataList, out ISceneData sceneData)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException($"Attempted to get an {nameof(ISceneData)} through an invalid or unloaded scene.");
            }

            foreach (ISceneData tempSceneData in sceneDataList)
            {
                if (scene == tempSceneData.SceneReference)
                {
                    sceneData = tempSceneData;
                    return true;
                }
            }

            Debug.LogWarning($"Unable to get an {nameof(ISceneData)} with the loaded scene {scene.name} ({scene.handle}).");
            sceneData = default;
            return false;
        }

        /// <summary>
        /// Gets whether all <see cref="IAsyncSceneOperation"/>s have completed in an array of <see cref="ISceneData"/>.
        /// </summary>
        public static bool HasCompletedAllSceneLoadOperations(ISceneData[] sceneDataArray)
        {
            int length = sceneDataArray.Length;
            for (int i = 0; i < length; i++)
            {
                if (!sceneDataArray[i].AsyncOperation.IsDone)
                    return false;
            }
            return true;
        }
    }
}