using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonData : MonoBehaviour
{
    private Image image;

    public bool active;
    public Artifact artifact;
    public string description;
    public Sprite activeSprite, inactiveSprite;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        updateImage();
    }

    public void updateImage()
    {
        Debug.Log(gameObject.name + " updating image");
        image.sprite = active ? activeSprite : inactiveSprite;
    }
}
