using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    GameMasterScript gameMasterScript;

    void Start(){
        gameMasterScript = GameObject.Find("/GameMaster").GetComponent<GameMasterScript>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
           gameMasterScript.ShowScore();
        }
    }
}
