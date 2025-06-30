using System.Collections;
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
    [TitleGroup("背景音乐设置")]
    [InfoBox("拖拽背景音乐文件到下面的列表中")]
    [LabelText("背景音乐列表")]
    #else
    [Header("背景音乐")]
    #endif
    public AudioClip[] backgroundMusicClips = new AudioClip[0];
    
    #if ODIN_INSPECTOR
    [Range(0f, 1f)]
    [OnValueChanged("OnVolumeChanged")]
    [LabelText("背景音乐音量")]
    #else
    [Range(0f, 1f)]
    #endif
    public float backgroundMusicVolume = 0.8f;
    
    #if ODIN_INSPECTOR
    [TitleGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("音效数量")]
    public int SoundCount => soundClips?.Length ?? 0;
    
    [ShowInInspector, ReadOnly]
    [LabelText("是否已初始化")]
    public bool IsInitialized => soundDict != null;
    
    [ShowInInspector, ReadOnly]
    [LabelText("背景音乐播放状态")]
    public bool IsBackgroundMusicPlaying => isBackgroundMusicPlaying;
    
    [ShowInInspector, ReadOnly]
    [LabelText("当前背景音乐")]
    public string CurrentBackgroundMusic => currentBackgroundMusic?.name ?? "无";
    #endif
    
    private AudioSource audioSource;
    private AudioSource backgroundAudioSource;
    private Dictionary<string, SoundClip> soundDict;
    
    // 背景音乐状态
    private bool isBackgroundMusicPlaying = false;
    private AudioClip currentBackgroundMusic = null;
    private Coroutine fadeCoroutine;
    
    private void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            
            // 确保背景音乐AudioSource正确初始化
            if (backgroundAudioSource == null)
            {
                Debug.LogError("[SimpleAudioManager] backgroundAudioSource 初始化失败");
            }
            else
            {
                Debug.Log("[SimpleAudioManager] 背景音乐系统初始化成功");
            }
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
        
        // 添加背景音乐AudioSource组件
        if (backgroundAudioSource == null)
        {
            backgroundAudioSource = gameObject.AddComponent<AudioSource>();
            backgroundAudioSource.loop = true;
            backgroundAudioSource.playOnAwake = false;
            backgroundAudioSource.volume = backgroundMusicVolume * masterVolume;
        }
        
        // 创建音效字典
        RefreshSoundDictionary();
        
        #if ODIN_INSPECTOR && UNITY_EDITOR
        Debug.Log($"<color=green>音效系统初始化完成，共加载 {soundDict.Count} 个音效</color>");
        Debug.Log($"<color=green>背景音乐系统初始化完成</color>");
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
        
        // 更新背景音乐音量
        if (Application.isPlaying)
        {
            UpdateBackgroundMusicVolume();
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
        UpdateBackgroundMusicVolume();
    }
    
    #region 背景音乐控制方法
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="musicIndex">音乐在backgroundMusicClips数组中的索引</param>
    /// <param name="fadeInDuration">淡入时长（秒），默认为0</param>
    public void PlayBackgroundMusic(int musicIndex, float fadeInDuration = 0f)
    {
        if (backgroundMusicClips == null || musicIndex < 0 || musicIndex >= backgroundMusicClips.Length)
        {
            Debug.LogWarning($"[SimpleAudioManager] 无效的音乐索引: {musicIndex}");
            return;
        }
        
        AudioClip musicClip = backgroundMusicClips[musicIndex];
        if (musicClip == null)
        {
            Debug.LogWarning($"[SimpleAudioManager] 索引 {musicIndex} 的音乐文件为空");
            return;
        }
        
        PlayBackgroundMusicInternal(musicClip, fadeInDuration);
    }
    
    /// <summary>
    /// 通过音乐名称播放背景音乐
    /// </summary>
    /// <param name="musicName">音乐文件名称（不含扩展名）</param>
    /// <param name="fadeInDuration">淡入时长（秒），默认为0</param>
    public void PlayBackgroundMusicByName(string musicName, float fadeInDuration = 0f)
    {
        if (string.IsNullOrEmpty(musicName))
        {
            Debug.LogWarning("[SimpleAudioManager] 音乐名称不能为空");
            return;
        }
        
        if (backgroundMusicClips == null)
        {
            Debug.LogWarning("[SimpleAudioManager] 背景音乐列表为空");
            return;
        }
        
        AudioClip foundClip = null;
        foreach (var clip in backgroundMusicClips)
        {
            if (clip != null && clip.name == musicName)
            {
                foundClip = clip;
                break;
            }
        }
        
        if (foundClip == null)
        {
            Debug.LogWarning($"[SimpleAudioManager] 找不到名为 '{musicName}' 的背景音乐");
            return;
        }
        
        PlayBackgroundMusicInternal(foundClip, fadeInDuration);
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    /// <param name="fadeOutDuration">淡出时长（秒），默认为0</param>
    public void StopBackgroundMusic(float fadeOutDuration = 0f)
    {
        if (!isBackgroundMusicPlaying || backgroundAudioSource == null)
        {
            return;
        }
        
        if (fadeOutDuration <= 0f)
        {
            backgroundAudioSource.Stop();
            isBackgroundMusicPlaying = false;
            currentBackgroundMusic = null;
            
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            Debug.Log("[SimpleAudioManager] 背景音乐已停止");
        }
        else
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeBackgroundMusic(0f, fadeOutDuration, true));
        }
    }
    
    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBackgroundMusic()
    {
        if (isBackgroundMusicPlaying && backgroundAudioSource != null)
        {
            backgroundAudioSource.Pause();
            Debug.Log("[SimpleAudioManager] 背景音乐已暂停");
        }
    }
    
    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBackgroundMusic()
    {
        if (isBackgroundMusicPlaying && backgroundAudioSource != null)
        {
            backgroundAudioSource.UnPause();
            Debug.Log("[SimpleAudioManager] 背景音乐已恢复");
        }
    }
    
    /// <summary>
    /// 设置背景音乐音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        UpdateBackgroundMusicVolume();
    }
    
    #endregion
    
    #region 背景音乐内部方法
    
    /// <summary>
    /// 内部播放背景音乐方法
    /// </summary>
    private void PlayBackgroundMusicInternal(AudioClip musicClip, float fadeInDuration)
    {
        if (backgroundAudioSource == null)
        {
            Debug.LogError("[SimpleAudioManager] backgroundAudioSource 未初始化");
            return;
        }
        
        // 如果正在播放相同的音乐，则不重复播放
        if (isBackgroundMusicPlaying && currentBackgroundMusic == musicClip)
        {
            Debug.Log($"[SimpleAudioManager] 背景音乐 '{musicClip.name}' 已在播放中");
            return;
        }
        
        // 停止当前音乐
        if (isBackgroundMusicPlaying)
        {
            backgroundAudioSource.Stop();
        }
        
        // 停止之前的淡入淡出效果
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // 设置新音乐
        backgroundAudioSource.clip = musicClip;
        currentBackgroundMusic = musicClip;
        isBackgroundMusicPlaying = true;
        
        if (fadeInDuration <= 0f)
        {
            UpdateBackgroundMusicVolume();
            backgroundAudioSource.Play();
            Debug.Log($"[SimpleAudioManager] 开始播放背景音乐: {musicClip.name}");
        }
        else
        {
            backgroundAudioSource.volume = 0f;
            backgroundAudioSource.Play();
            fadeCoroutine = StartCoroutine(FadeBackgroundMusic(backgroundMusicVolume * masterVolume, fadeInDuration, false));
            Debug.Log($"[SimpleAudioManager] 开始淡入播放背景音乐: {musicClip.name}");
        }
    }
    
    /// <summary>
    /// 淡入/淡出背景音乐协程
    /// </summary>
    private IEnumerator FadeBackgroundMusic(float targetVolume, float duration, bool stopAfterFade)
    {
        if (backgroundAudioSource == null) yield break;
        
        float startVolume = backgroundAudioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            backgroundAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        backgroundAudioSource.volume = targetVolume;
        
        if (stopAfterFade)
        {
            backgroundAudioSource.Stop();
            isBackgroundMusicPlaying = false;
            currentBackgroundMusic = null;
            Debug.Log("[SimpleAudioManager] 背景音乐淡出完成并停止");
        }
        
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// 更新背景音乐音量
    /// </summary>
    private void UpdateBackgroundMusicVolume()
    {
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = backgroundMusicVolume * masterVolume;
        }
    }
    
    #endregion
    
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
    
    [TitleGroup("背景音乐调试工具")]
    [Button("播放测试背景音乐", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.8f, 1f)]
    public void TestPlayBackgroundMusic([ValueDropdown("GetBackgroundMusicIndices")] int musicIndex)
    {
        PlayBackgroundMusic(musicIndex);
    }
    
    [TitleGroup("背景音乐调试工具")]
    [Button("停止背景音乐", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.8f)]
    public void TestStopBackgroundMusic()
    {
        StopBackgroundMusic();
    }
    
    [TitleGroup("背景音乐调试工具")]
    [Button("暂停/恢复背景音乐", ButtonSizes.Medium)]
    [GUIColor(1f, 1f, 0.8f)]
    public void TestToggleBackgroundMusic()
    {
        if (isBackgroundMusicPlaying && backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            PauseBackgroundMusic();
        }
        else if (isBackgroundMusicPlaying && backgroundAudioSource != null)
        {
            ResumeBackgroundMusic();
        }
    }
    
    private IEnumerable<int> GetBackgroundMusicIndices()
    {
        if (backgroundMusicClips == null) yield break;
        
        for (int i = 0; i < backgroundMusicClips.Length; i++)
        {
            yield return i;
        }
    }
    
    private IEnumerable<string> GetBackgroundMusicNames()
    {
        if (backgroundMusicClips == null) yield break;
        
        foreach (var clip in backgroundMusicClips)
        {
            if (clip != null)
            {
                yield return clip.name;
            }
        }
    }
    
    [TitleGroup("背景音乐调试工具")]
    [Button("列出所有背景音乐", ButtonSizes.Large)]
    [GUIColor(0.8f, 1f, 1f)]
    public void ListAllBackgroundMusic()
    {
        if (backgroundMusicClips == null || backgroundMusicClips.Length == 0)
        {
            Debug.Log("<color=yellow>没有可用的背景音乐</color>");
            return;
        }
        
        Debug.Log("<color=green>=== 所有可用背景音乐 ===</color>");
        for (int i = 0; i < backgroundMusicClips.Length; i++)
        {
            var clip = backgroundMusicClips[i];
            if (clip != null)
            {
                string info = $"索引: {i}, 名称: {clip.name}, 时长: {clip.length:F1}s";
                Debug.Log($"<color=white>{info}</color>");
            }
            else
            {
                Debug.Log($"<color=red>索引: {i}, 名称: 空</color>");
            }
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
