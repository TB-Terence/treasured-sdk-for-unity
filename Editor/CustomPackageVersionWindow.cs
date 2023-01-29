using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasured.UnitySdk
{
    public class CustomPackageVersionWindow : EditorWindow
    {
        private const string k_Owner = "TB-Terence";
        private const string k_Repo = "treasured-sdk-for-unity";
        private const string k_authorizationTokenHelpUri = "https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token";
     
        private struct GitHubBranch
        {
            public string name;
        }

        public class Styles
        {
            public static readonly GUIContent info = EditorGUIUtility.TrIconContent("Info", "How to get Authorization Token");
        }

#if TREASURED_SDK_DEVMODE
        [MenuItem("Tools/Treasured/Custom Package Version", priority = 99)]
#endif
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<CustomPackageVersionWindow>();
            window.titleContent = new GUIContent("Choose Custom Version");
            window.Show();
        }

        private string _authorizationToken;

        private GitHubBranch[] _branches = new GitHubBranch[0];

        private int _selectedIndex;

        private string _error;

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Authorization Token");
                EditorGUILayout.Space();
                if (GUILayout.Button(Styles.info, EditorStyles.label, GUILayout.MaxWidth(24)))
                {
                    EditorUtility.OpenWithDefaultApp(k_authorizationTokenHelpUri);
                }
            }
            _authorizationToken = EditorGUILayout.TextArea(_authorizationToken, EditorStyles.textArea, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 3));
            using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(_authorizationToken)))
            {
                if (GUILayout.Button("Fetch Branches"))
                {
                    GetBranches();
                }
            }
            using (new EditorGUI.DisabledGroupScope(_branches.Length == 0))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _selectedIndex = EditorGUILayout.Popup(new GUIContent("Branches"), _selectedIndex, _branches.Select(x => x.name).ToArray());
                    if (GUILayout.Button("Switch", GUILayout.Width(96)))
                    {
                        string branchName = _branches[_selectedIndex].name;
                        if(!string.IsNullOrEmpty(branchName))
                            Client.Add($"https://github.com/{k_Owner}/{k_Repo}.git#{branchName}");
                        this.Close();
                    }
                }
            }
            if (!string.IsNullOrEmpty(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
        }

        private async void GetBranches()
        {
            using (var www = UnityWebRequest.Get($"https://api.github.com/repos/{k_Owner}/{k_Repo}/branches"))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Accept", "application/vnd.github+json");
                www.SetRequestHeader("Authorization", $"Bearer {_authorizationToken}");
                www.SetRequestHeader("X-GitHub-Api-Version", $"2022-11-28");

                var asyncOperation = www.SendWebRequest();

                while(!asyncOperation.isDone)
                    await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    _branches = JsonConvert.DeserializeObject<GitHubBranch[]>(www.downloadHandler.text);
                    _error = string.Empty;
                }
                else
                {
                    _error = www.error;
                }
            }
        }
    }
}
