using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("要激活的GameObject（过场动画播放完成后激活）")]
    public GameObject targetGameObject;

    [Tooltip("过场动画GameObject")]
    public GameObject cutsceneGameObject;

    // 公开方法，供按钮调用
    public void StartGame()
    {
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

    public void QuitGame()
    {
        // 打印日志，因为在编辑器中 Application.Quit() 无效
        Debug.Log("游戏退出指令已发出");
        Application.Quit();

        // 预编译指令：如果在编辑器模式下，则停止播放
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}