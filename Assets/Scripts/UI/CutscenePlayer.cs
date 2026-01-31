using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CutscenePlayer : MonoBehaviour
{
    [Tooltip("过场图片")]
    [SerializeField]
    private Image cutsceneImage;

    [Tooltip("过场文本")]
    [SerializeField]
    private TMP_Text cutsceneText;

    [Tooltip("过场图片列表")]
    [SerializeField]
    private List<Sprite> imageList = new List<Sprite>();

    [Tooltip("过场文本列表（与图片列表一一对应）")]
    [SerializeField]
    private List<string> textList = new List<string>();

    [Tooltip("文本播放完成后的等待时间（秒）")]
    [SerializeField]
    private float waitTimeAfterText = 2f;

    [Tooltip("第一个图片的透明度渐变时间（秒）")]
    [SerializeField]
    private float fadeInDuration = 1f;

    [Tooltip("过场动画播放完成后要激活的GameObject")]
    public GameObject targetGameObject;

    private Coroutine playCoroutine;
    private int currentIndex = 0;
    private TypewriterEffect typewriterEffect;

    private void Awake()
    {
        // 获取或添加TypewriterEffect组件
        if (cutsceneText != null)
        {
            typewriterEffect = cutsceneText.GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                typewriterEffect = cutsceneText.gameObject.AddComponent<TypewriterEffect>();
            }
            
            // 禁用TypewriterEffect组件，防止它在Start()中自动播放
            // 我们会在需要时通过SetTextAndPlay()手动控制
            typewriterEffect.enabled = false;
            
            // 清空TMP文本
            cutsceneText.text = "";
        }
    }

    private void Start()
    {
        Debug.Log("CutscenePlayer: Start() 被调用");
        
        // 确保TypewriterEffect不会自动播放
        if (typewriterEffect != null)
        {
            typewriterEffect.StopTypewriter();
        }
        
        // 只有在targetGameObject已设置时才自动播放（避免在MainMenuController设置之前就开始播放）
        // 如果targetGameObject为null，等待外部调用Play()
        if (targetGameObject != null)
        {
            Debug.Log("CutscenePlayer: targetGameObject已设置，开始播放过场动画");
            Play();
        }
        else
        {
            Debug.Log("CutscenePlayer: targetGameObject未设置，等待外部调用Play()");
        }
    }

    private void OnEnable()
    {
        Debug.Log("CutscenePlayer: OnEnable() 被调用");
        // 如果GameObject被重新激活，只有在targetGameObject已设置时才重新开始播放
        if (playCoroutine == null && targetGameObject != null)
        {
            Play();
        }
    }

    /// <summary>
    /// 播放过场动画
    /// </summary>
    public void Play()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        Debug.Log("CutscenePlayer: PlayCoroutine() 开始执行");

        // 检查列表是否为空
        if (imageList == null || textList == null || imageList.Count == 0 || textList.Count == 0)
        {
            Debug.LogWarning("CutscenePlayer: 图片列表或文本列表为空！直接激活targetGameObject");
            // 如果列表为空，等待一帧后直接激活targetGameObject
            yield return null;
            if (targetGameObject != null)
            {
                targetGameObject.SetActive(true);
            }
            yield break;
        }

        Debug.Log($"CutscenePlayer: 共有 {imageList.Count} 组内容需要播放");

        // 重置索引
        currentIndex = 0;

        // 遍历所有图片和文本
        while (currentIndex < imageList.Count && currentIndex < textList.Count)
        {
            Debug.Log($"CutscenePlayer: 显示第 {currentIndex + 1} 组内容");

            // 设置当前图片
            if (cutsceneImage != null)
            {
                if (imageList[currentIndex] != null)
                {
                    cutsceneImage.sprite = imageList[currentIndex];
                    cutsceneImage.gameObject.SetActive(true);

                    // 第一个图片添加透明度渐变效果
                    if (currentIndex == 0)
                    {
                        // 设置初始透明度为0
                        Color color = cutsceneImage.color;
                        color.a = 0f;
                        cutsceneImage.color = color;

                        // 渐变到完全不透明
                        float elapsedTime = 0f;
                        while (elapsedTime < fadeInDuration)
                        {
                            elapsedTime += Time.deltaTime;
                            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                            color.a = alpha;
                            cutsceneImage.color = color;
                            yield return null;
                        }

                        // 确保最终完全不透明
                        color.a = 1f;
                        cutsceneImage.color = color;
                    }
                    else
                    {
                        // 后续图片直接显示，不应用渐变
                        Color color = cutsceneImage.color;
                        color.a = 1f;
                        cutsceneImage.color = color;
                    }
                }
                else
                {
                    Debug.LogWarning($"CutscenePlayer: 第 {currentIndex + 1} 组图片为空");
                }
            }

            // 等待一帧，确保Image已经显示
            yield return null;

            // 设置当前文本并使用打字机效果显示
            if (cutsceneText != null)
            {
                cutsceneText.gameObject.SetActive(true);
                
                // 使用TypewriterEffect来逐个显示文字
                if (typewriterEffect != null)
                {
                    // 启用TypewriterEffect组件（之前被禁用了）
                    typewriterEffect.enabled = true;
                    // 先停止之前的打字机效果（如果有）
                    typewriterEffect.StopTypewriter();
                    // 设置新文本并开始播放
                    typewriterEffect.SetTextAndPlay(textList[currentIndex]);
                }
                else
                {
                    // 如果没有TypewriterEffect，直接显示全部文字
                    cutsceneText.text = textList[currentIndex];
                    cutsceneText.ForceMeshUpdate();
                    cutsceneText.maxVisibleCharacters = cutsceneText.textInfo.characterCount;
                }
            }

            // 等待打字机效果完成
            if (typewriterEffect != null && cutsceneText != null)
            {
                cutsceneText.ForceMeshUpdate();
                int totalCharacters = cutsceneText.textInfo.characterCount;
                
                while (cutsceneText.maxVisibleCharacters < totalCharacters)
                {
                    yield return null;
                    cutsceneText.ForceMeshUpdate();
                    totalCharacters = cutsceneText.textInfo.characterCount;
                }
            }
            else
            {
                // 如果没有打字机效果，等待一帧
                yield return null;
            }

            // 等待配置的时间后再切换到下一组
            yield return new WaitForSeconds(waitTimeAfterText);

            // 切换到下一组
            currentIndex++;
        }

        Debug.Log("CutscenePlayer: 所有过场动画播放完毕");
        
        // 等待一帧，确保所有动画效果都已完成
        yield return null;
        
        // 激活目标GameObject
        if (targetGameObject != null)
        {
            Debug.Log($"CutscenePlayer: 激活目标GameObject: {targetGameObject.name}");
            targetGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("CutscenePlayer: targetGameObject 未设置，无法激活");
        }
    }
}
