using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static ES3; // 引入Easy Save 3命名空间

public class SettingsManager : MonoBehaviour
{
    [Header("UI 组件引用")]
    public Slider musicSlider; // 拖拽赋值
    public Slider sfxSlider;   // 拖拽赋值
    public GameObject settingsPanel; // 设置面板对象

    [Header("音频混音器")]
    public AudioMixer mainMixer;

    // 使用常量字符串作为Key，防止拼写错误
    private const string KEY_MUSIC = "MusicVolume";
    private const string KEY_SFX = "SFXVolume";
    private const string MIXER_MUSIC = "MusicVol"; // 必须与Mixer中Expose的名字一致
    private const string MIXER_SFX = "SFXVol";

    // ES3保存的文件名
    private const string SAVE_FILE = "SettingsData.json";

    private void Start()
    {
        // 1. 初始化：读取保存的数据，如果没有则默认为1（最大音量）
        float savedMusic = LoadVolume(KEY_MUSIC, 1f);
        float savedSFX = LoadVolume(KEY_SFX, 1f);

        // 2. 更新UI状态
        musicSlider.value = savedMusic;
        sfxSlider.value = savedSFX;

        // 3. 应用音量到Mixer
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        // 4. 绑定事件监听
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 5. 初始隐藏设置面板
        settingsPanel.SetActive(false);
    }

    // 使用ES3加载音量值
    private float LoadVolume(string key, float defaultValue)
    {
        // 检查键是否存在，如果存在则返回值，否则返回默认值
        if (ES3.KeyExists(key, SAVE_FILE))
        {
            return ES3.Load<float>(key, SAVE_FILE);
        }
        return defaultValue;
    }

    // 设置背景音乐音量
    public void SetMusicVolume(float value)
    {
        // 对数转换公式：Log10(value) * 20
        float dB = Mathf.Log10(value) * 20;

        // 设置Mixer参数
        mainMixer.SetFloat(MIXER_MUSIC, dB);

        // 使用ES3保存数据到JSON文件
        ES3.Save(KEY_MUSIC, value, SAVE_FILE);
    }

    // 设置音效音量
    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Log10(value) * 20;
        mainMixer.SetFloat(MIXER_SFX, dB);

        // 使用ES3保存数据到JSON文件
        ES3.Save(KEY_SFX, value, SAVE_FILE);
    }

    // 删除保存的数据（可选功能）
    public void DeleteSavedData()
    {
        if (ES3.FileExists(SAVE_FILE))
        {
            ES3.DeleteFile(SAVE_FILE);
            Debug.Log("设置数据已删除");

            // 重置为默认值
            musicSlider.value = 1f;
            sfxSlider.value = 1f;
            SetMusicVolume(1f);
            SetSFXVolume(1f);
        }
    }

    // 打开/关闭设置面板的辅助方法
    public void ToggleSettings(bool isOpen)
    {
        // 播放按钮音效
        PlayButtonSound();
        
        settingsPanel.SetActive(isOpen);
    }

    // 重载方法，可以接收按钮GameObject作为参数
    public void ToggleSettings(bool isOpen, GameObject buttonObj)
    {
        // 播放按钮音效
        PlayButtonSound(buttonObj);
        ToggleSettings(isOpen);
    }

    /// <summary>
    /// 播放按钮音效（通过EventSystem获取当前被点击的按钮）
    /// </summary>
    private void PlayButtonSound()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            PlayButtonSound(EventSystem.current.currentSelectedGameObject);
        }
    }

    /// <summary>
    /// 播放按钮音效（通过传入的按钮GameObject）
    /// </summary>
    private void PlayButtonSound(GameObject buttonObj)
    {
        if (buttonObj != null)
        {
            AudioSource audioSource = buttonObj.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                Debug.Log($"SettingsManager: 已播放按钮音效 - {buttonObj.name}");
            }
        }
    }
}
