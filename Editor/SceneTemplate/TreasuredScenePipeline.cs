using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Treasured.UnitySdk
{
    public class TreasuredScenePipeline : ISceneTemplatePipeline
    {
        private static readonly List<Scene> _scenes = new List<Scene>();
        
        public bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset) => true;

        public void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset,
            bool isAdditive,
            string sceneName)
        {
        }

        public void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset,
            Scene scene,
            bool isAdditive,
            string sceneName)
        {
            // _scenes.Add(scene);
            //
            // EditorSceneManager.sceneSaving += OnSceneSaving;
            // EditorSceneManager.MarkSceneDirty(scene);
            // EditorSceneManager.SaveScene(scene);
            // AssetDatabase.Refresh();
        }

        private void OnSceneSaving(Scene scene, string path)
        {
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            var registered = _scenes.FirstOrDefault(s => s == scene);
            if (registered.IsValid())
            {
                _scenes.Remove(registered);
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                };
            }
        }
    }
}
