using System;
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
    private Artifact equippedItem = null;
    private TextMeshProUGUI itemDescription;
    [SerializeField] private float useArtifactPeriod, inventorySize = 9f;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private List<ButtonData> displayButtons;
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
        if (displayButtons.Count == inventorySize)
            return false;
        
        AddItemToDisplay(item);

        return true;
    }

    public void displayDescription(String desc)
    {
        itemDescription.text = desc;
    }

    public void EquipItem(Artifact artifact, bool active)
    {
        //Deactivate current item if changing
        if (equippedItem && artifact != equippedItem)
        {
            Debug.Log("?");
            equippedItem.deactivate();
            ButtonData button = findButtonWithItem(equippedItem);
            button.active = false;
            button.updateImage();
        }

        equippedItem = artifact;
        Debug.Log("New: " + artifact);
        if (active)
        {
            if(equippedItem.activate())
                RemoveItem(equippedItem);
        }
        else
        {
            equippedItem.deactivate();
        }
    }

    public void RemoveItem(Artifact item)
    {
        equippedItem = null;
        displayDescription("");
        ButtonData toRemove = findButtonWithItem(item);
        displayButtons.Remove(toRemove);
        Destroy(toRemove.gameObject);
    }

    private ButtonData findButtonWithItem(Artifact item)
    {
        foreach (ButtonData button in displayButtons)
        {
            if (button.artifact == item)
            {
                Debug.Log(item + ": Found");
                return button;
            }
        }

        Debug.Log(item + ": Not found");
        return null;
    }

    private void AddItemToDisplay(Artifact item)
    {
        GameObject button = Instantiate(buttonPrefab, inventoryDisplay.transform);
        ButtonData data = button.GetComponent<ButtonData>();
        displayButtons.Add(data);
        data.setup(this, item);
    }
}
