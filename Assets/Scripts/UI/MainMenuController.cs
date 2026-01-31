using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用，用于场景加载

public class MainMenuController : MonoBehaviour
{
    // 公开方法，供按钮调用
    public void StartGame()
    {
        // 加载索引为1的场景，或者使用场景名
        // 确保在 Build Settings 中添加了场景
        SceneManager.LoadScene(1);
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