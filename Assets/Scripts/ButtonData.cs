using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonData : MonoBehaviour, IPointerClickHandler
{
    private Image image;
    private GameObject border;
    private Color borderColor;
    private Inventory_scr playerInventory;

    public bool active = false;
    public Artifact artifact;
    public string description;
    public Sprite activeSprite, inactiveSprite;
    
    private void Awake()
    {
        borderColor = transform.GetChild(0).GetComponent<Image>().color;
        image = GetComponent<Image>();
        image.sprite = activeSprite;
        updateImage();
    }

    public void updateImage()
    {
        borderColor.a = active ? 1 : .02f;

        borderColor = transform.GetChild(0).GetComponent<Image>().color = borderColor;
    }
    
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            playerInventory.displayDescription(description);
        }
        else if (eventData.clickCount == 2) {
            active = !active;
            playerInventory.EquipItem(artifact, active);
            updateImage();
        }
    }

    public void setup(Inventory_scr inventoryScr, Artifact artifact)
    {
        playerInventory = inventoryScr;
        this.artifact = artifact;
        description = artifact.m_description;
        activeSprite = artifact.m_spriteActive;
        inactiveSprite = artifact.m_spriteInactive;
        gameObject.name = artifact.m_name;
    }
}
