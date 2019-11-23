using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

public class Inventory_scr : MonoBehaviour
{
    public List<Artifact> allArtifacts; //Add in editor
    private List<Artifact> playerInventory;
    private Artifact equippedItem = null;
    private TextMeshProUGUI itemDescription;
    [SerializeField] private float useArtifactPeriod, inventorySize = 9f;
    [SerializeField] private GameObject buttonPrefab;
    private List<ButtonData> displayButtons;
    private GameObject characterSheet, inventoryDisplay;
    private bool characterSheetOpen = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        characterSheet = GameObject.FindWithTag("CharacterUI");
        inventoryDisplay = GameObject.FindWithTag("InventoryUI");
        displayButtons = new List<ButtonData>();
        itemDescription = GameObject.FindWithTag("InventoryUIText").GetComponent<TextMeshProUGUI>();
        itemDescription.text = "";
        Hide();
        playerInventory = new List<Artifact>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            if(characterSheetOpen)
                Hide();
            else
                Show();
        }
    }

    public void printInventory()
    {
        Debug.Log("------------------");
        string inventory = "";
        foreach (var artifactScr in allArtifacts)
        {
            inventory += artifactScr.ToString() + " ";
        }
        Debug.Log(inventory);
        Debug.Log("------------------");
    }

    private void Show()
    {
        characterSheet.SetActive(true);
        characterSheetOpen = true;
        Time.timeScale = 0f;
        PauseMenu_scr.gameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Hide()
    {
        characterSheet.SetActive(false);
        characterSheetOpen = false;
        Time.timeScale = 1f;
        PauseMenu_scr.gameIsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool AddItem(Artifact item)
    {
        if (playerInventory.Count == inventorySize)
            return false;
        
        playerInventory.Add(item);
        AddItemToDisplay(item);

        return true;
    }

    private void EquipItem(ButtonData data)
    {
        //Deactivate current item if changing
        if (equippedItem && data.artifact != equippedItem)
        {
            equippedItem.deactivate();
            ButtonData button = findButtonWithItem(equippedItem);
            button.active = false;
            button.updateImage();
        }

        equippedItem = data.artifact;
        data.active = !data.active;

        if (data.active)
        {
            itemDescription.text = equippedItem.ToString();
            
            if(equippedItem.activate())
                RemoveItem(equippedItem);
        }
        else
        {
            itemDescription.text = "";

            equippedItem.deactivate();
        }
        
        data.updateImage();
    }

    public void RemoveItem(Artifact item)
    {
        playerInventory.Remove(item);
        equippedItem = null;

        Destroy(findButtonWithItem(item).gameObject);
    }

    private ButtonData findButtonWithItem(Artifact item)
    {
        foreach (ButtonData button in displayButtons)
        {
            if (button.artifact == item)
                return button;
        }

        return null;
    }

    private void AddItemToDisplay(Artifact item)
    {
        GameObject button = Instantiate(buttonPrefab, inventoryDisplay.transform);
        ButtonData data = button.GetComponent<ButtonData>();
        displayButtons.Add(data);

        data.activeSprite = item.m_spriteActive;
        data.inactiveSprite = item.m_spriteInactive;
        data.description = item.m_description;
        data.artifact = item;
        data.active = false;
        
        button.GetComponent<Button>().onClick.AddListener(delegate { EquipItem(data); });
    }
}
