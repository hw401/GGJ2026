using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("要激活的GameObject")]
    public GameObject targetGameObject;

    // 公开方法，供按钮调用
    public void StartGame()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MainMenuController: targetGameObject 未赋值");
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