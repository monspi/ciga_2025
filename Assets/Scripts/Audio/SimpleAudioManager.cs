using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

// 音频配置
[System.Serializable]
public class SoundClip
{
    #if ODIN_INSPECTOR
    [HorizontalGroup("Sound", 0.3f)]
    [LabelWidth(50)]
    #endif
    public string name = "新音效";        // 音效名称
    
    #if ODIN_INSPECTOR
    [HorizontalGroup("Sound", 0.5f)]
    [AssetsOnly]
    [LabelWidth(50)]
    #endif
    public AudioClip clip;     // 音频文件
    
    #if ODIN_INSPECTOR
    [HorizontalGroup("Sound", 0.2f)]
    [Range(0f, 1f)]
    [LabelWidth(50)]
    #else
    [Range(0f, 1f)]
    #endif
    public float volume = 1f;  // 音量
    
    #if ODIN_INSPECTOR
    [ShowInInspector, ReadOnly]
    [HorizontalGroup("Info")]
    [ShowIf("clip")]
    public string Duration => clip != null ? $"{clip.length:F1}s" : "未设置";
    
    [ShowInInspector, ReadOnly]
    [HorizontalGroup("Info")]
    [ShowIf("clip")]
    public string Format => clip != null ? $"{clip.frequency}Hz" : "";
    #endif
}

// 简单音效管理器
public class SimpleAudioManager : MonoBehaviour
{
    public static SimpleAudioManager Instance;
    
    #if ODIN_INSPECTOR
    [TitleGroup("音效系统设置")]
    [InfoBox("拖拽音频文件到下面的列表中，给每个音效起个名字即可使用")]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [LabelText("音效列表")]
    #else
    [Header("音效列表")]
    #endif
    public SoundClip[] soundClips = new SoundClip[0];
    
    #if ODIN_INSPECTOR
    [TitleGroup("全局设置")]
    [Range(0f, 1f)]
    [OnValueChanged("OnVolumeChanged")]
    #else
    [Header("设置")]
    [Range(0f, 1f)]
    #endif
    public float masterVolume = 1f;
    
    #if ODIN_INSPECTOR
    [TitleGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("音效数量")]
    public int SoundCount => soundClips?.Length ?? 0;
    
    [ShowInInspector, ReadOnly]
    [LabelText("是否已初始化")]
    public bool IsInitialized => soundDict != null;
    #endif
    
    private AudioSource audioSource;
    private Dictionary<string, SoundClip> soundDict;
    
    private void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // 添加AudioSource组件
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 创建音效字典
        RefreshSoundDictionary();
        
        #if ODIN_INSPECTOR && UNITY_EDITOR
        Debug.Log($"<color=green>音效系统初始化完成，共加载 {soundDict.Count} 个音效</color>");
        #endif
    }
    
    #if ODIN_INSPECTOR
    [TitleGroup("调试工具")]
    [Button("刷新音效字典", ButtonSizes.Medium)]
    [GUIColor(0.7f, 1f, 0.7f)]
    #endif
    public void RefreshSoundDictionary()
    {
        soundDict = new Dictionary<string, SoundClip>();
        
        if (soundClips == null) return;
        
        foreach (var sound in soundClips)
        {
            if (sound != null && !string.IsNullOrEmpty(sound.name))
            {
                if (soundDict.ContainsKey(sound.name))
                {
                    Debug.LogWarning($"重复的音效名称: {sound.name}");
                }
                else
                {
                    soundDict[sound.name] = sound;
                }
            }
        }
    }
    
    #if ODIN_INSPECTOR
    [TitleGroup("调试工具")]
    [Button("播放测试音效", ButtonSizes.Medium)]
    [GUIColor(0.7f, 0.7f, 1f)]
    public void PlayTestSound([ValueDropdown("GetSoundNames")] string soundName)
    {
        PlaySound(soundName);
    }
    
    private IEnumerable<string> GetSoundNames()
    {
        if (soundClips == null) yield break;
        
        foreach (var sound in soundClips)
        {
            if (sound != null && !string.IsNullOrEmpty(sound.name))
            {
                yield return sound.name;
            }
        }
    }
    
    private void OnVolumeChanged()
    {
        // 当音量在编辑器中改变时的回调
        if (Application.isPlaying && audioSource != null)
        {
            audioSource.volume = masterVolume;
        }
    }
    #endif
    
    // 播放音效
    public void PlaySound(string soundName)
    {
        Debug.Log($"[SimpleAudioManager] PlaySound() 被调用，音效名称: {soundName}");
        
        if (soundDict == null)
        {
            Debug.Log("[SimpleAudioManager] soundDict 为 null，尝试初始化");
            Initialize();
        }
        
        Debug.Log($"[SimpleAudioManager] 当前 soundDict 中有 {soundDict?.Count ?? 0} 个音效");
        
        if (soundDict.ContainsKey(soundName))
        {
            SoundClip sound = soundDict[soundName];
            if (sound.clip != null)
            {
                audioSource.PlayOneShot(sound.clip, sound.volume * masterVolume);
                Debug.Log($"[SimpleAudioManager] 成功播放音效: {soundName}");
                
                #if ODIN_INSPECTOR && UNITY_EDITOR
                Debug.Log($"<color=cyan>播放音效: {soundName}</color>");
                #endif
            }
            else
            {
                Debug.LogWarning($"[SimpleAudioManager] 音效 '{soundName}' 的AudioClip为空!");
            }
        }
        else
        {
            string availableSounds = soundDict?.Count > 0 ? string.Join(", ", soundDict.Keys) : "无";
            Debug.LogWarning($"[SimpleAudioManager] 音效 '{soundName}' 不存在! 可用音效: {availableSounds}");
        }
    }
    
    // 设置主音量
    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
    
    #if ODIN_INSPECTOR
    [TitleGroup("调试工具")]
    [Button("列出所有音效", ButtonSizes.Large)]
    [GUIColor(1f, 1f, 0.7f)]
    public void ListAllSounds()
    {
        if (soundDict == null || soundDict.Count == 0)
        {
            Debug.Log("<color=yellow>没有可用的音效</color>");
            return;
        }
        
        Debug.Log("<color=green>=== 所有可用音效 ===</color>");
        foreach (var kvp in soundDict)
        {
            var sound = kvp.Value;
            string info = $"名称: {kvp.Key}, 时长: {(sound.clip ? sound.clip.length.ToString("F1") + "s" : "N/A")}, 音量: {sound.volume:F1}";
            Debug.Log($"<color=white>{info}</color>");
        }
    }
    #endif
}

// 音效触发器
public class AudioTrigger : MonoBehaviour
{
    #if ODIN_INSPECTOR
    [TitleGroup("触发设置")]
    [ValueDropdown("GetAvailableSounds")]
    [LabelText("音效名称")]
    #else
    [Header("音效设置")]
    #endif
    public string soundName = "";
    
    #if ODIN_INSPECTOR
    [TitleGroup("触发方式")]
    [LabelText("游戏开始时播放")]
    #endif
    public bool playOnStart = false;
    
    #if ODIN_INSPECTOR
    [LabelText("激活时播放")]
    #endif
    public bool playOnEnable = false;
    
    #if ODIN_INSPECTOR
    [LabelText("鼠标点击播放")]
    #endif
    public bool playOnClick = true;
    
    #if ODIN_INSPECTOR
    [LabelText("碰撞触发播放")]
    #endif
    public bool playOnTrigger = false;
    
    #if ODIN_INSPECTOR
    [ShowIf("playOnTrigger")]
    [LabelText("触发标签")]
    #endif
    public string triggerTag = "Player";
    
    #if ODIN_INSPECTOR
    private IEnumerable<string> GetAvailableSounds()
    {
        if (SimpleAudioManager.Instance == null || SimpleAudioManager.Instance.soundClips == null)
        {
            yield return "无可用音效";
            yield break;
        }
        
        foreach (var sound in SimpleAudioManager.Instance.soundClips)
        {
            if (sound != null && !string.IsNullOrEmpty(sound.name))
            {
                yield return sound.name;
            }
        }
    }
    
    [TitleGroup("测试工具")]
    [Button("播放测试", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void TestPlaySound()
    {
        PlaySound();
    }
    #endif
    
    private void Start()
    {
        if (playOnStart)
        {
            PlaySound();
        }
    }
    
    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlaySound();
        }
    }
    
    // 播放音效
    public void PlaySound()
    {
        if (SimpleAudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
        {
            SimpleAudioManager.Instance.PlaySound(soundName);
        }
        else
        {
            Debug.LogWarning("AudioManager不存在或音效名称为空!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (playOnTrigger && other.CompareTag(triggerTag))
        {
            PlaySound();
        }
    }
    
    private void OnMouseDown()
    {
        if (playOnClick)
        {
            PlaySound();
        }
    }
}
