#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(SimpleAudioManager))]
public class SimpleAudioManagerEditor : OdinEditor
{
    private string audioFolderPath = "Assets/Audio";
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        DrawAutoLoadSection();
    }
    
    private void DrawAutoLoadSection()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎵 自动加载工具", EditorStyles.boldLabel);
        
        // 文件夹路径设置
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("音效文件夹:", GUILayout.Width(80));
        audioFolderPath = EditorGUILayout.TextField(audioFolderPath);
        if (GUILayout.Button("选择文件夹", GUILayout.Width(80)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择音效文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 转换为相对于项目的路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    audioFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // 自动加载按钮
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎵 自动加载所有OGG音效", GUILayout.Height(35)))
        {
            AutoLoadAudioFiles();
        }
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("🔄 追加加载OGG音效", GUILayout.Height(35)))
        {
            AutoLoadAudioFiles(true);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // 显示当前文件夹信息
        if (Directory.Exists(audioFolderPath))
        {
            var oggFiles = Directory.GetFiles(audioFolderPath, "*.ogg", SearchOption.AllDirectories);
            EditorGUILayout.HelpBox($"找到 {oggFiles.Length} 个OGG文件", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"文件夹不存在: {audioFolderPath}", MessageType.Warning);
        }
        
        // 显示项目音效文件夹信息
        EditorGUILayout.Space(5);
        string projectAudioPath = "Assets/Audio";
        if (Directory.Exists(projectAudioPath))
        {
            var projectOggFiles = Directory.GetFiles(projectAudioPath, "*.ogg", SearchOption.AllDirectories);
            EditorGUILayout.HelpBox($"项目Audio文件夹中有 {projectOggFiles.Length} 个OGG文件", MessageType.Info);
            
            if (projectOggFiles.Length > 0 && GUILayout.Button("🎯 快速加载项目Audio文件夹", GUILayout.Height(30)))
            {
                audioFolderPath = projectAudioPath;
                AutoLoadAudioFiles();
            }
        }
    }
    
    private void AutoLoadAudioFiles(bool append = false)
    {
        var manager = (SimpleAudioManager)target;
        
        if (!Directory.Exists(audioFolderPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在: {audioFolderPath}", "确定");
            return;
        }
        
        // 查找所有OGG文件
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { audioFolderPath });
        var audioClips = new List<AudioClip>();
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.ToLower().EndsWith(".ogg"))
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    audioClips.Add(clip);
                }
            }
        }
        
        if (audioClips.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", $"在文件夹 {audioFolderPath} 中未找到OGG音效文件", "确定");
            return;
        }
        
        // 创建新的音效配置
        var newSoundClips = new List<SoundClip>();
        
        // 如果是追加模式，先添加现有的
        if (append && manager.soundClips != null)
        {
            newSoundClips.AddRange(manager.soundClips);
        }
        
        int addedCount = 0;
        // 添加新找到的音效
        foreach (var clip in audioClips)
        {
            // 检查是否已存在（避免重复）
            if (append && manager.soundClips != null)
            {
                bool exists = manager.soundClips.Any(s => s.clip == clip);
                if (exists) continue;
            }
            
            string fileName = Path.GetFileNameWithoutExtension(clip.name);
            newSoundClips.Add(new SoundClip
            {
                name = fileName,
                clip = clip,
                volume = 1f
            });
            addedCount++;
        }
        
        manager.soundClips = newSoundClips.ToArray();
        manager.RefreshSoundDictionary();
        
        EditorUtility.SetDirty(manager);
        
        string message = append ? 
            $"追加加载完成！新增 {addedCount} 个音效，总计 {manager.soundClips.Length} 个" :
            $"自动加载完成！共加载 {addedCount} 个OGG音效文件";
            
        EditorUtility.DisplayDialog("完成", message, "确定");
        
        Debug.Log($"<color=green>{message}</color>");
        
        // 列出加载的音效名称
        Debug.Log("<color=cyan>=== 已加载的音效列表 ===</color>");
        foreach (var sound in manager.soundClips)
        {
            Debug.Log($"<color=white>• {sound.name}</color>");
        }
    }
}

#else
// 不使用Odin的标准编辑器
[CustomEditor(typeof(SimpleAudioManager))]
public class SimpleAudioManagerEditor : Editor
{
    private string audioFolderPath = "Assets/Audio";
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎵 自动加载工具", EditorStyles.boldLabel);
        
        // 文件夹路径设置
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("音效文件夹:", GUILayout.Width(80));
        audioFolderPath = EditorGUILayout.TextField(audioFolderPath);
        if (GUILayout.Button("选择", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择音效文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    audioFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // 自动加载按钮
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎵 自动加载所有OGG音效", GUILayout.Height(35)))
        {
            AutoLoadAudioFiles();
        }
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("🔄 追加加载OGG音效", GUILayout.Height(35)))
        {
            AutoLoadAudioFiles(true);
        }
        GUI.backgroundColor = Color.white;
        
        // 显示文件夹信息
        if (Directory.Exists(audioFolderPath))
        {
            var oggFiles = Directory.GetFiles(audioFolderPath, "*.ogg", SearchOption.AllDirectories);
            EditorGUILayout.HelpBox($"找到 {oggFiles.Length} 个OGG文件", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"文件夹不存在: {audioFolderPath}", MessageType.Warning);
        }
        
        // 显示项目音效文件夹信息
        EditorGUILayout.Space(5);
        string projectAudioPath = "Assets/Audio";
        if (Directory.Exists(projectAudioPath))
        {
            var projectOggFiles = Directory.GetFiles(projectAudioPath, "*.ogg", SearchOption.AllDirectories);
            EditorGUILayout.HelpBox($"项目Audio文件夹中有 {projectOggFiles.Length} 个OGG文件", MessageType.Info);
            
            if (projectOggFiles.Length > 0 && GUILayout.Button("🎯 快速加载项目Audio文件夹", GUILayout.Height(30)))
            {
                audioFolderPath = projectAudioPath;
                AutoLoadAudioFiles();
            }
        }
    }
    
    private void AutoLoadAudioFiles(bool append = false)
    {
        var manager = (SimpleAudioManager)target;
        
        if (!Directory.Exists(audioFolderPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在: {audioFolderPath}", "确定");
            return;
        }
        
        // 查找所有OGG文件
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { audioFolderPath });
        var audioClips = new List<AudioClip>();
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.ToLower().EndsWith(".ogg"))
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    audioClips.Add(clip);
                }
            }
        }
        
        if (audioClips.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", $"在文件夹 {audioFolderPath} 中未找到OGG音效文件", "确定");
            return;
        }
        
        // 创建新的音效配置
        var newSoundClips = new List<SoundClip>();
        
        // 如果是追加模式，先添加现有的
        if (append && manager.soundClips != null)
        {
            newSoundClips.AddRange(manager.soundClips);
        }
        
        int addedCount = 0;
        // 添加新找到的音效
        foreach (var clip in audioClips)
        {
            // 检查是否已存在
            if (append && manager.soundClips != null)
            {
                bool exists = manager.soundClips.Any(s => s.clip == clip);
                if (exists) continue;
            }
            
            string fileName = Path.GetFileNameWithoutExtension(clip.name);
            newSoundClips.Add(new SoundClip
            {
                name = fileName,
                clip = clip,
                volume = 1f
            });
            addedCount++;
        }
        
        manager.soundClips = newSoundClips.ToArray();
        manager.RefreshSoundDictionary();
        
        EditorUtility.SetDirty(manager);
        
        string message = append ? 
            $"追加完成！新增 {addedCount} 个，总计 {manager.soundClips.Length} 个" :
            $"加载完成！共 {addedCount} 个OGG文件";
            
        EditorUtility.DisplayDialog("完成", message, "确定");
        
        Debug.Log($"<color=green>{message}</color>");
        
        // 列出加载的音效名称
        Debug.Log("<color=cyan>=== 已加载的音效列表 ===</color>");
        foreach (var sound in manager.soundClips)
        {
            Debug.Log($"<color=white>• {sound.name}</color>");
        }
    }
}
#endif
#endif