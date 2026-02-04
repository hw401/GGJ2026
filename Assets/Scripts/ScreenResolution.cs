using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    public int m_Width = 16;
    public int m_Height = 9;
 
    private int width;
    private int height;
 
 
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Vector2 resolution = VivewResolution();
        Screen.SetResolution((int)resolution.x, (int)resolution.y, true);
    }
 
 
    public Vector2 VivewResolution()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        float ratoWidth = currentWidth / m_Width;
        float ratoHeight = currentHeight / m_Height;
 
        if (ratoWidth > ratoHeight)
        {
            height = Screen.height;
            width = currentHeight / m_Height * m_Width;
        }
        else if (ratoWidth < ratoHeight)
        {
            height = currentWidth / m_Width * m_Height;
            width = Screen.width;
        }
        else
        {
            height = Screen.height;
            width = Screen.width;
        }
        return new Vector2(width, height);
    }
}
