using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

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
		ZombieAttributes.attackMod++;
		
		PlayerController_scr player = GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>();
		foreach (SummonAIControl minion in player.minions)		
			minion.ModifyAttributes(attack: 1);
		foreach (SummonAIControl minion in player.minionsAway)		
			minion.ModifyAttributes(attack: 1);
		
		Debug.Log("Minionatt: " + ZombieAttributes.attackMod);
	}

	private void IncreaseMinionSpeed()
	{
		ZombieAttributes.moveSpeedMod++;
		
		PlayerController_scr player = GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>();
		foreach (SummonAIControl minion in player.minions)		
			minion.ModifyAttributes(moveSpeed: 1);
		foreach (SummonAIControl minion in player.minionsAway)		
			minion.ModifyAttributes(moveSpeed: 1);
		
		Debug.Log("Minion speed: " + ZombieAttributes.moveSpeedMod);
	}
	
	private void IncreaseMinionHealth()
	{
		ZombieAttributes.healthMod++;
		
		PlayerController_scr player = GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>();
		foreach (SummonAIControl minion in player.minions)		
			minion.ModifyAttributes(health: 1);
		foreach (SummonAIControl minion in player.minionsAway)		
			minion.ModifyAttributes(health: 1);
		
		Debug.Log("Minion health: " + ZombieAttributes.healthMod);
	}
	
	private void IncreaseMinionAttackSpeed()
	{
		ZombieAttributes.attackSpeedMod++;
		
		PlayerController_scr player = GameObject.FindWithTag("Player").GetComponent<PlayerController_scr>();
		foreach (SummonAIControl minion in player.minions)		
			minion.ModifyAttributes(attackSpeed: 1);
		foreach (SummonAIControl minion in player.minionsAway)		
			minion.ModifyAttributes(attackSpeed: 1);
		
		Debug.Log("Minion Aspd: " + ZombieAttributes.attackSpeedMod);
	}

	public void RegisterButton(LevelUp panel, int function)
	{
		Debug.Log(function);
		levelUpPanel = panel;
		buttonFunction = function;
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = buttonNames[buttonFunction];
	}
}