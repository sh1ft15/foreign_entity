using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    [SerializeField] Transform diceRolledObj, moveMadeObj, totalDamageObj;
    [SerializeField] int diceRolled, moveMade, totalDamage;

    public void ToggleScore(bool status) {
        gameObject.SetActive(status);
    }

    public void UpdateDiceRolled(int num) {
        diceRolled = Mathf.Max(diceRolled + num, 0);
    }

    public void UpdateMoveMade(int num) {
        moveMade = Mathf.Max(moveMade + num, 0);
    }

    public void UpdateTotalDamage(int num) {
        totalDamage = Mathf.Max(totalDamage + num, 0);
    }

    public void UpdateContent(){
        diceRolledObj.Find("Num").GetComponent<Text>().text = diceRolled.ToString();
        moveMadeObj.Find("Num").GetComponent<Text>().text = moveMade.ToString();
        totalDamageObj.Find("Num").GetComponent<Text>().text = totalDamage.ToString();
    }
}
