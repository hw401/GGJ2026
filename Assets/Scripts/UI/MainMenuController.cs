using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("要激活的GameObject（过场动画播放完成后激活）")]
    public GameObject targetGameObject;

    [Tooltip("过场动画GameObject（游玩场景转场）")]
    public GameObject cutsceneGameObject;

    [Tooltip("演职人员表转场GameObject")]
    public GameObject castCutsceneGameObject;

    // 公开方法，供按钮调用
    public void StartGame()
    {
        // 播放按钮音效
        PlayButtonSound();
        
        // 先激活过场动画
        if (cutsceneGameObject != null)
        {
            // 获取CutscenePlayer组件并设置targetGameObject
            CutscenePlayer cutscenePlayer = cutsceneGameObject.GetComponent<CutscenePlayer>();
            if (cutscenePlayer != null)
            {
                // 设置过场动画播放完成后要激活的GameObject
                cutscenePlayer.targetGameObject = targetGameObject;
                
                // 如果过场动画GameObject已经激活，手动调用Play()确保开始播放
                if (cutsceneGameObject.activeSelf)
                {
                    cutscenePlayer.Play();
                }
            }
            
            // 激活过场动画GameObject（如果还没激活，Start()会自动调用Play()）
            cutsceneGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MainMenuController: cutsceneGameObject 未赋值，直接激活targetGameObject");
            // 如果没有过场动画，直接激活目标GameObject
            if (targetGameObject != null)
            {
                targetGameObject.SetActive(true);
            }
        }
    }

    // 重载方法，可以接收按钮GameObject作为参数
    public void StartGame(GameObject buttonObj)
    {
        // 播放按钮音效
        PlayButtonSound(buttonObj);
        StartGame();
    }

    public void QuitGame()
    {
        // 播放按钮音效
        PlayButtonSound();
        
        // 打印日志，因为在编辑器中 Application.Quit() 无效
        Debug.Log("游戏退出指令已发出");
        Application.Quit();

        // 预编译指令：如果在编辑器模式下，则停止播放
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 重载方法，可以接收按钮GameObject作为参数
    public void QuitGame(GameObject buttonObj)
    {
        // 播放按钮音效
        PlayButtonSound(buttonObj);
        QuitGame();
    }

    /// <summary>
    /// 打开演职人员表（激活演职人员表转场）
    /// </summary>
    public void OpenCast()
    {
        // 播放按钮音效
        PlayButtonSound();
        
        // 激活演职人员表转场
        if (castCutsceneGameObject != null)
        {
            // 获取CutscenePlayer组件
            CutscenePlayer cutscenePlayer = castCutsceneGameObject.GetComponent<CutscenePlayer>();
            if (cutscenePlayer != null)
            {
                // 如果转场GameObject已经激活，手动调用Play()确保开始播放
                if (castCutsceneGameObject.activeSelf)
                {
                    cutscenePlayer.Play();
                }
            }
            
            // 激活转场GameObject（如果还没激活，Start()会自动调用Play()）
            castCutsceneGameObject.SetActive(true);
            Debug.Log("MainMenuController: 已激活演职人员表转场");
        }
        else
        {
            Debug.LogWarning("MainMenuController: castCutsceneGameObject 未赋值，无法打开演职人员表");
        }
    }

    // 重载方法，可以接收按钮GameObject作为参数
    public void OpenCast(GameObject buttonObj)
    {
        // 播放按钮音效
        PlayButtonSound(buttonObj);
        OpenCast();
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
                Debug.Log($"MainMenuController: 已播放按钮音效 - {buttonObj.name}");
            }
        }
    }
}