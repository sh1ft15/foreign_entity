using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfectedScript : MonoBehaviour
{
    [SerializeField] Transform healthBar;
    [SerializeField] Animator animator;
    [SerializeField] ItemScriptObject itemObject;
    [SerializeField] float health, maxHealth;
    GameMasterScript gameMasterScript;
    SpriteRenderer spriteRend;
    Transform curPlatform, player;
    bool facingRight = true, isDeath;

    void Start(){
        gameMasterScript = GameObject.Find("/GameMaster").GetComponent<GameMasterScript>();
        spriteRend = transform.Find("Character/Sprite").GetComponent<SpriteRenderer>();
        player = GameObject.Find("/Player").transform;

        if (itemObject != null) { SetIdleStance(itemObject.itemName); }

        UpdateHealth(0);
    }

    void OnMouseOver() {
        if (!gameMasterScript.InCombat() && !isDeath) { 
            bool canTraverse = gameMasterScript.MobInRange(curPlatform);

            spriteRend.color = canTraverse ? Color.green : Color.red;

            if (Input.GetMouseButtonUp(0) && canTraverse) {
                gameMasterScript.ToggleCombat(transform, true);
            }
        }
    }

    void OnMouseExit() {
        spriteRend.color = Color.white; 
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Platform") { curPlatform = other.transform; }
    }

    public bool CheckForPlayer() {
        if (curPlatform != null) {
            int range = itemObject != null ? itemObject.range : 1,
                curIndex = curPlatform.GetSiblingIndex(),
                playerIndex = gameMasterScript.GetPlayerIndex(),
                indexDiff = Mathf.Abs(playerIndex - curIndex);

            return indexDiff <= range;
        }
        else { return false; }
    }

    public void Flip(float horizontal) {
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight)) {
            facingRight = !facingRight;
            transform.Find("Character").rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        }
    }

    public void SetIdleStance(string stance) {
        animator.Play("idle_" + stance);
    }

    public void UpdateHealth(float num) {
        Vector2 scale = healthBar.Find("Image").localScale;
        health = Mathf.Max(Mathf.Min(health + num, maxHealth), 0);

        scale.x = health / maxHealth;
        healthBar.Find("Image").localScale = scale;

        if (health <= 0 && !isDeath) { StartCoroutine(AnimateDeath()); }
    }

    IEnumerator AnimateDeath() {
        if (isDeath == false) {
            spriteRend.color = Color.black;
            isDeath = true;

            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }
    }

    public ItemScriptObject GetItemObject() { return itemObject; }

    public void AnimateAttack() { animator.SetTrigger("attack"); }
}
