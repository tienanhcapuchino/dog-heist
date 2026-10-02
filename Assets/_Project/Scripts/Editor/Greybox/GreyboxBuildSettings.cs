using System.Collections.Generic;
using UnityEditor;

namespace DogHeist.EditorTools.Greybox
{
    /// <summary>Đưa scene vào danh sách build (cần cho nút Chơi lại), không thêm trùng.</summary>
    internal static class GreyboxBuildSettings
    {
        public static void EnsureSceneInBuild(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
