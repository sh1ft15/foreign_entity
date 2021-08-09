using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [SerializeField] ItemScriptObject itemObject;
    GameMasterScript gameMasterScript;

    void Start() {
        gameMasterScript = GameObject.Find("/GameMaster").GetComponent<GameMasterScript>();
        transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = itemObject.sprite;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            if (itemObject.itemName.Equals("medkit")) {
                gameMasterScript.UpdateHealth(5);
            }
            else { gameMasterScript.PopulateInventory(itemObject); }

            Destroy(gameObject);
        }
    }
}
