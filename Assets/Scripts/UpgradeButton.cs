using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerClickHandler
{
	TextMeshProUGUI buttonText;
	private Button button;
	private LevelUp levelUpPanel;
	private PlayerAttributes_scr playerAttributes;
	private string[] buttonNames = new[]
	{
		"Recover Health", "++Minion Speed", "++Minion Damage",
		"++Minion Health", "++Minion Attack Speed"
	};	
	private int buttonFunction;
	
	private void Start()
	{		
		playerAttributes = GameObject.FindWithTag("GameManager").GetComponent<PlayerAttributes_scr>();
	}

	private void selectButton()
	{		
		switch(buttonFunction){
			case 0:
				Heal();
				break;				
			case 1:
				IncreaseMinionSpeed();
				break;
			case 2:
				IncreaseMinionAttack();
				break;
			case 3:
				IncreaseMinionHealth();
				break;
			case 4:
				IncreaseMinionAttackSpeed();
				break;
		}           
	}
	
	public virtual void OnPointerClick(PointerEventData eventData)
	{			
		selectButton();
		levelUpPanel.ClosePanel();
	}
	
	private void Heal(){
		Debug.Log("Heal");
		playerAttributes.heal();
	}

	private void IncreaseMaxMinions(){
		//This is poor, upgrading every level now instead.
		Debug.Log("Max minion");
		playerAttributes.summonsLimit *= 2;
		playerAttributes.updateHud();
	}

	private void IncreaseMinionAttack(){
		Debug.Log("Minionatt");
		ZombieAttributes.attackMod++;
	}

	private void IncreaseMinionSpeed()
	{
		Debug.Log("Minion speed");
		ZombieAttributes.moveSpeedMod++;
	}
	
	private void IncreaseMinionHealth()
	{
		Debug.Log("Minion health");
		ZombieAttributes.healthMod++;
	}
	
	private void IncreaseMinionAttackSpeed()
	{
		Debug.Log("Minion Aspd");
		ZombieAttributes.attackSpeedMod++;
	}

	public void RegisterButton(LevelUp panel, int function)
	{
		Debug.Log(function);
		levelUpPanel = panel;
		buttonFunction = function;
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = buttonNames[buttonFunction];
	}
}