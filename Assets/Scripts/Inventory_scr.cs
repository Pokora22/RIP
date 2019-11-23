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
    public static List<Artifact_scr> allArtifacts = new List<Artifact_scr>();
    private List<Artifact_scr> playerInventory;
    [SerializeField] private float useArtifactPeriod;
    [SerializeField] private GameObject buttonPrefab;
    private GameObject characterSheet, inventoryDisplay;
    private bool characterSheetOpen = false;
    
    // Start is called before the first frame update
    void Awake()
    {
        CreateArtifacts();
        characterSheet = GameObject.FindWithTag("CharacterUI");
        inventoryDisplay = GameObject.FindWithTag("InventoryUI");
        Hide();
        playerInventory = new List<Artifact_scr>();
    }

    private void CreateArtifacts()
    {
        for(int i = 0; i < 10; i++)
            allArtifacts.Add(new Artifact_scr("Some item"));
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

    public bool AddItem(Artifact_scr item)
    {
        allArtifacts.Add(item);
        AddItemToDisplay(item);

        return true;
    }

    private void EquipItem(GameObject source)
    {
        Debug.Log(source.GetComponent<Artifact_scr>());
        //TODO: Add real activation methods
    }

    private void AddItemToDisplay(Artifact_scr item)
    {
        GameObject button = Instantiate(buttonPrefab, inventoryDisplay.transform);
        Artifact_scr buttonStats = button.AddComponent<Artifact_scr>();
        buttonStats.m_description = item.m_description;
        buttonStats.m_sprite = item.m_sprite;
        
        button.GetComponent<Button>().onClick.AddListener(delegate { EquipItem(button); });
        button.GetComponent<Image>().color = Color.red;
        
        Debug.Log("Inventory updated ");
    }
    
    private IEnumerator useArtifacts()
    {
        yield return new WaitForSeconds(useArtifactPeriod);
        foreach (Artifact_scr artifact in playerInventory)
            if (!artifact.useAbility())
                playerInventory.Remove(artifact);
    }
}
