using HHG.Audio.Runtime;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HHG.Audio.Editor
{
    public class AudioClipMenuItem
    {
        [MenuItem("CONTEXT/AudioClip/Create Sound Groups")]
        private static void CreateSoundGroups(MenuCommand cmd)
        {
            if (cmd.context != Selection.activeObject)
            {
                return;
            }
            string folder = EditorUtility.OpenFolderPanel("Select Folder to Save Sound Groups", "Assets", "");

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
                SoundGroupAsset soundGroup = ScriptableObject.CreateInstance<SoundGroupAsset>();
                soundGroup.Sounds.Add(new Sound(guid));

                string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{clip.name}.asset");
                AssetDatabase.CreateAsset(soundGroup, path);
                AssetDatabase.SaveAssets();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sfx Groups Created", "Successfully created Sound Groups for the selected Audio Clips.", "OK");
        }
    }
}
