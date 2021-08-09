using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    [SerializeField] string mode = "normal";
    [SerializeField] Animator animator;
    GameMasterScript gameMasterScript;
    SpriteRenderer spriteRend;
    Transform curObject;
    bool facingRight = true;

    void Start() {
        spriteRend = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        gameMasterScript = GameObject.Find("/GameMaster").GetComponent<GameMasterScript>();

        UpdateMode(mode);
    }

    void OnMouseOver() {
        if (curObject == null) {
            bool canTraverse = gameMasterScript.CheckPlatform(transform);
            int moveCount = gameMasterScript.GetMoveCount(transform);

            spriteRend.color = canTraverse ? Color.green : Color.red;

            if (Input.GetMouseButtonUp(0) && canTraverse) {
                gameMasterScript.SetPlayerNextPlatform(transform);
                gameMasterScript.UpdateMoveCount(-moveCount);
            }
        }
        else if (curObject.tag == "Player") {
            switch(mode){
                case "hole":
                case "vent":
                    bool canTraverse = gameMasterScript.GetMoveLeft() > 0;

                    if (Input.GetMouseButtonUp(0) && canTraverse) {
                        Transform pair = gameMasterScript.GetPlatformPair(transform);

                        gameMasterScript.SetPlayerNextPlatform(pair, true);
                        gameMasterScript.UpdateMoveCount(-1);
                    }

                    spriteRend.color = canTraverse ? Color.green : Color.red;
                break;
            }
        }
    }

    void OnMouseExit() {
        spriteRend.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D other) {
        switch(other.tag) {
            case "Player":
            case "Monster": 
            case "Mob": curObject = other.transform; break;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        switch(other.tag) {
            case "Player":
            case "Monster": 
            case "Mob": curObject = null; break;
        }
    }

    public Transform GetObject(){ return curObject; }

    public void UpdateMode(string str) {
        mode = str;
        animator.Play("idle_" + mode);
    }

    public string GetMode() { return mode; }
}
