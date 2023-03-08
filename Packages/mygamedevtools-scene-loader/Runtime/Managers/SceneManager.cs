#if ENABLE_UNITASK && !OVERRIDE_DISABLE_UNITASK
#define USE_UNITASK
#endif
/**
 * SceneManager.cs
 * Created by: João Borks [joao.borks@gmail.com]
 * Created on: 2022-12-21
 */

#if USE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

[assembly: InternalsVisibleTo("MyGameDevTools.SceneLoading.Tests")]

namespace MyGameDevTools.SceneLoading
{
    public class SceneManager : ISceneManager, ISceneManagerReporter
    {
        public event Action<Scene, Scene> ActiveSceneChanged;
        public event Action<Scene> SceneUnloaded;
        public event Action<Scene> SceneLoaded;

        public bool IsUnloadingScenes => _unloadingScenes.Count > 0;
        public int SceneCount => _loadedScenes.Count;

        readonly List<Scene> _unloadingScenes = new List<Scene>();
        readonly List<Scene> _loadedScenes = new List<Scene>();

        Scene _activeScene;

        public void SetActiveScene(Scene scene)
        {
            var validScene = scene.IsValid();
            if (validScene && !_loadedScenes.Contains(scene))
                throw new InvalidOperationException($"Cannot set active the scene \"{scene.name}\" that has not been loaded through this {GetType().Name}.");

            var previousScene = _activeScene;
            _activeScene = scene;
            if (validScene)
                UnitySceneManager.SetActiveScene(scene);

            ActiveSceneChanged?.Invoke(previousScene, scene);
        }

        public Scene GetActiveScene() => _activeScene;

        public Scene GetLastLoadedScene()
        {
            if (SceneCount == 0)
                return default;

            for (int i = SceneCount - 1; i >= 0; i--)
                if (!_unloadingScenes.Contains(_loadedScenes[i]) && _loadedScenes[i].isLoaded)
                    return _loadedScenes[i];

            return default;
        }

        public Scene GetLoadedSceneAt(int index) => _loadedScenes[index];

        public Scene GetLoadedSceneByName(string name)
        {
            foreach (var scene in _loadedScenes)
                if (scene.name == name)
                    return scene;
            throw new ArgumentException($"Could not find any loaded scene with the name '{name}'.", nameof(name));
        }

        public async ValueTask<Scene> LoadSceneAsync(ILoadSceneInfo sceneInfo, bool setActive = false, IProgress<float> progress = null)
        {
            var operation = GetLoadSceneOperation(sceneInfo);

#if USE_UNITASK
            await operation.ToUniTask(progress);
#else
            while (!operation.isDone)
            {
                await Task.Yield();
                progress?.Report(operation.progress);
            }
#endif

            var loadedScene = GetLastUnityLoadedSceneByInfo(sceneInfo);

            _loadedScenes.Add(loadedScene);
            SceneLoaded?.Invoke(loadedScene);

            if (setActive)
                SetActiveScene(loadedScene);

            return loadedScene;
        }

        public async ValueTask<Scene> UnloadSceneAsync(ILoadSceneInfo sceneInfo)
        {
            var scene = GetLastSceneByInfo(sceneInfo);
            if (_unloadingScenes.Contains(scene))
                return await WaitForSceneUnload(scene);
            if (!_loadedScenes.Contains(scene))
                throw new InvalidOperationException($"Cannot unload the scene \"{scene.name}\" that has not been loaded through this {GetType().Name}.");

            _loadedScenes.Remove(scene);
            _unloadingScenes.Add(scene);
            var operation = GetUnloadSceneOperation(sceneInfo);
#if USE_UNITASK
            await operation.ToUniTask();
#else
            while (!operation.isDone)
                await Task.Yield();
#endif

            _unloadingScenes.Remove(scene);
            if (_activeScene == scene)
                SetActiveScene(GetLastLoadedScene());

            SceneUnloaded?.Invoke(scene);

            return scene;
        }

        async ValueTask<Scene> WaitForSceneUnload(Scene scene)
        {
#if USE_UNITASK
            await UniTask.WaitUntil(() => !_unloadingScenes.Contains(scene));
#else
            while (_unloadingScenes.Contains(scene))
                await Task.Yield();
#endif
            return scene;
        }

        AsyncOperation GetLoadSceneOperation(ILoadSceneInfo sceneInfo)
        {
            if (sceneInfo.Reference is int index)
                return UnitySceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            else if (sceneInfo.Reference is string name)
                return UnitySceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            else if (sceneInfo.Reference is Scene scene)
                return UnitySceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive);
            else
                throw new Exception($"Unexpected {nameof(ILoadSceneInfo.Reference)} type.");
        }

        AsyncOperation GetUnloadSceneOperation(ILoadSceneInfo sceneInfo)
        {
            if (sceneInfo.Reference is int index)
                return UnitySceneManager.UnloadSceneAsync(index);
            else if (sceneInfo.Reference is string name)
                return UnitySceneManager.UnloadSceneAsync(name);
            else if (sceneInfo.Reference is Scene scene)
                return UnitySceneManager.UnloadSceneAsync(scene);
            else
                throw new Exception($"Unexpected {nameof(ILoadSceneInfo.Reference)} type.");
        }

        Scene GetLastSceneByInfo(ILoadSceneInfo sceneInfo)
        {
            var sceneCount = SceneCount;
            int i;
            for (i = sceneCount - 1; i >= 0; i--)
            {
                var scene = _loadedScenes[i];
                if (sceneInfo.IsReferenceToScene(scene))
                    return scene;
            }

            sceneCount = _unloadingScenes.Count;
            for (i = 0; i < sceneCount; i++)
            {
                var scene = _unloadingScenes[i];
                if (sceneInfo.IsReferenceToScene(scene))
                    return scene;
            }
            throw new ArgumentException($"Could not find any scene with the provided ILoadSceneInfo: {sceneInfo}");
        }

        Scene GetLastUnityLoadedSceneByInfo(ILoadSceneInfo sceneInfo)
        {
            var sceneCount = UnitySceneManager.sceneCount;
            for (int i = sceneCount - 1; i >= 0; i--)
            {
                var scene = UnitySceneManager.GetSceneAt(i);
                if (scene.isLoaded && sceneInfo.IsReferenceToScene(scene))
                    return scene;
            }
            throw new ArgumentException($"Could not find any loaded scene in the Unity Scene Manager with the provided ILoadSceneInfo: {sceneInfo}");
        }
    }
}