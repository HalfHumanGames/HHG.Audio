using HHG.Audio.Runtime;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HHG.Audio.Editor
{
    public class AudioClipMenuItem
    {
        [MenuItem("CONTEXT/AudioClip/Create Sfx Groups")]
        private static void CreateSfxGroups(MenuCommand cmd)
        {
            if (cmd.context != Selection.activeObject)
            {
                return;
            }
            string folder = EditorUtility.OpenFolderPanel("Select Folder to Save Sfx Groups", "Assets", "");

            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (folder.StartsWith(Application.dataPath))
            {
                folder = "Assets" + folder.Substring(Application.dataPath.Length);
            }

            foreach (AudioClip clip in Selection.objects.OfType<AudioClip>())
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
                SfxGroupAsset sfxGroup = ScriptableObject.CreateInstance<SfxGroupAsset>();
                sfxGroup.Sfxs.Add(new Sfx(guid));

                string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{clip.name}.asset");
                AssetDatabase.CreateAsset(sfxGroup, path);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sfx Groups Created", "Successfully created Sfx Groups for the selected Audio Clips.", "OK");
        }
    }
}
