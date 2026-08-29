using HHG.Audio.Runtime;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HHG.Audio.Editor
{
    public class AudioClipMenuItem
    {
        [MenuItem("Assets/Tools/Audio/Create Sound Group Per Clip")]
        private static void CreateSoundGroups()
        {
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

        [MenuItem("Assets/Tools/Audio/Create Sound Group Per Clip", true)]
        private static bool CanCreateSoundGroups()
        {
            return Selection.objects.OfType<AudioClip>().Any();
        }

        [MenuItem("Assets/Tools/Audio/Create Combined Sound Group")]
        private static void CreateCombinedSoundGroup()
        {
            AudioClip[] clips = Selection.objects.OfType<AudioClip>().ToArray();

            if (clips.Length == 0)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Combined Sound Group", "Sound Group", "asset", "Choose where to save the combined Sound Group");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            SoundGroupAsset soundGroup = ScriptableObject.CreateInstance<SoundGroupAsset>();

            foreach (AudioClip clip in clips)
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
                soundGroup.Sounds.Add(new Sound(guid));
            }

            AssetDatabase.CreateAsset(soundGroup, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sound Group Created", "Successfully created a combined Sound Group from the selected Audio Clips.", "OK");
        }

        [MenuItem("Assets/Tools/Audio/Create Combined Sound Group", true)]
        private static bool CanCreateCombinedSoundGroup()
        {
            return Selection.objects.OfType<AudioClip>().Any();
        }
    }
}
