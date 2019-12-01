using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerClickHandler
{
	TextMeshProUGUI buttonText;
	private Button button;
	private LevelUp levelUpPanel;

	private string[] buttonNames;
	private int buttonFunction;
	
	private void Start()
	{
		buttonNames = new[]
		{
			"Recover Health", "++Maximum Minions", "++Minion Speed", "++Minion Damage",
			"++Minion Health"
		};
		button = gameObject.GetComponent<Button>();
		buttonText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		buttonFunction = Random.Range(0, 5);
		buttonText.text = buttonNames[buttonFunction];
	}

	private void selectButton()
	{		
		switch(buttonFunction){
			case 0:
				Heal();
				break;
			case 1:
				IncreaseMaxMinions();
				break;
			case 2:
				IncreaseMinionSpeed();
				break;
			case 3:
				IncreaseMinionAttack();
				break;
			case 4:
				IncreaseMinionHealth();
				break;
		}           
	}
	
	public virtual void OnPointerClick(PointerEventData eventData)
	{		
		Debug.Log(gameObject + " clicked");
		selectButton();
		levelUpPanel.ClosePanel();
	}
	
	private void Heal(){
		Debug.Log("Heal");
	}

	private void IncreaseMaxMinions(){
		Debug.Log("Max minion");
	}

	private void IncreaseMinionAttack(){
		Debug.Log("Minionatt");
	}

	private void IncreaseMinionSpeed()
	{
		Debug.Log("Minion speed");
	}
	
	private void IncreaseMinionHealth()
	{
		Debug.Log("Minion health");
	}

	public void RegisterPanel(LevelUp panel)
	{
		levelUpPanel = panel;
	}
}