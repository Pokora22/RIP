using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonData : MonoBehaviour
{
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public bool active;
    public Artifact artifact;
    public string description;
    public Sprite activeSprite, inactiveSprite;

    public void updateImage()
    {
        image.sprite = active ? activeSprite : inactiveSprite;
    }
}
