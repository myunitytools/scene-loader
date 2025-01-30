using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace MyGameDevTools.SceneLoading
{
    public static class UnityTaskUtilities
    {
        static Queue<Action> Actions;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void HookToPlayerLoop()
        {
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
            bool domainReloadDisabled = UnityEditor.EditorSettings.enterPlayModeOptionsEnabled && UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload);
            if (!domainReloadDisabled && Actions != null)
                return;
#else
            if (Actions != null)
                return;
#endif

            Actions = new Queue<Action>(16);
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            List<PlayerLoopSystem> updatedSystems = new(playerLoop.subSystemList)
            {
                new PlayerLoopSystem
                {
                    type = typeof(UnityTaskUtilities),
                    updateDelegate = ProcessMainThreadQueue
                }
            };

            playerLoop.subSystemList = updatedSystems.ToArray();
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static Task FromAsyncOperation(IAsyncSceneOperation asyncSceneOperation, CancellationToken token = default)
        {
            TaskCompletionSource<bool> tcs = new();

            token.Register(() =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetCanceled(token);
                }
            });

            Enqueue(() =>
            {
                if (tcs.Task.IsCanceled || tcs.Task.IsFaulted)
                    return;

                if (asyncSceneOperation.IsDone)
                {
                    tcs.SetResult(true);
                    return;
                }

                asyncSceneOperation.Completed += () =>
                {
                    tcs.TrySetResult(true);
                };
            });

            return tcs.Task;
        }

        static void Enqueue(Action action)
        {
            lock (Actions)
            {
                Actions.Enqueue(action);
            }
        }

        static void ProcessMainThreadQueue()
        {
            lock (Actions)
            {
                while (Actions.TryDequeue(out Action action))
                {
                    action.Invoke();
                }
            }
        }
    }
}