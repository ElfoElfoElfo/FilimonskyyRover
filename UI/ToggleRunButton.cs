using UnityEngine;
using UnityEngine.UI;
public class ToggleRunButton : MonoBehaviour
{
    public RoverController controller;
    
    [Header("Run button image")]
    public RawImage runButtonIcon;
    public Image runButtonImage;

    [Header("Run/stop textures & colors")]
    public Texture runTexture;
    public Texture stopTexture;
    public Color runColor, stopColor;

    public void Update()
    {
        if (!controller.animatingCode)
        {
            runButtonIcon.texture = runTexture;
            runButtonImage.color = runColor;
        }
        else
        {
            runButtonIcon.texture = stopTexture;
            runButtonImage.color = stopColor;
        }
    }
}
