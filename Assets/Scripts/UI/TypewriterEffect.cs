using UnityEngine;
using System.Collections;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [Tooltip("文本出现速度（字符/秒），数值越大速度越快")]
    [SerializeField]
    private float charactersPerSecond = 20f;

    private TMP_Text textComponent;
    private string fullText;
    private Coroutine typewriterCoroutine;

    private void Awake()
    {
        // 获取TMP组件
        textComponent = GetComponent<TMP_Text>();
        
        if (textComponent == null)
        {
            Debug.LogError("TypewriterEffect: 未找到TMP_Text组件！请确保此脚本挂载在有TextMeshPro或TextMeshProUGUI组件的GameObject上。");
            enabled = false;
            return;
        }

        // 保存完整文本
        fullText = textComponent.text;
    }

    private void Start()
    {
        StartTypewriter();
    }

    /// <summary>
    /// 开始打字机效果
    /// </summary>
    public void StartTypewriter()
    {
        if (textComponent == null) return;

        // 如果已经在播放，先停止
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // 重置文本
        textComponent.text = fullText;
        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = 0;

        // 开始协程
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine());
    }

    /// <summary>
    /// 停止打字机效果并显示完整文本
    /// </summary>
    public void StopTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (textComponent != null)
        {
            textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        }
    }

    /// <summary>
    /// 重置并重新开始打字机效果
    /// </summary>
    public void RestartTypewriter()
    {
        StopTypewriter();
        StartTypewriter();
    }

    /// <summary>
    /// 设置要显示的文本并开始打字机效果
    /// </summary>
    /// <param name="newText">新文本</param>
    public void SetTextAndPlay(string newText)
    {
        if (textComponent == null) return;

        fullText = newText;
        textComponent.text = fullText;
        StartTypewriter();
    }

    private IEnumerator TypewriterCoroutine()
    {
        if (textComponent == null) yield break;

        // 强制更新网格以获取有效的字符信息
        textComponent.ForceMeshUpdate();

        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        int visibleCount = 0;

        // 计算每个字符的显示间隔时间
        float delay = 1f / charactersPerSecond;

        while (visibleCount < totalVisibleCharacters)
        {
            visibleCount++;
            textComponent.maxVisibleCharacters = visibleCount;

            yield return new WaitForSeconds(delay);
        }
    }

    /// <summary>
    /// 获取或设置文本出现速度（字符/秒）
    /// </summary>
    public float CharactersPerSecond
    {
        get { return charactersPerSecond; }
        set 
        { 
            charactersPerSecond = Mathf.Max(0.1f, value); // 确保速度不为0或负数
        }
    }
}
