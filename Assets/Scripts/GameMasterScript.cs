using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Experimental.Rendering.Universal;

public class GameMasterScript : MonoBehaviour
{
    
    [SerializeField] Transform diceObject, playerTurnObject, enemyTurnObject, trailObject, gridObject, 
        propsObject, inventoryObject, anchorObject, healthBar, introObj;
    [SerializeField] float health, maxHealth;
    [SerializeField] List<Sprite> diceSprites;
    List<ItemScriptObject> itemObjects;
    List<Transform> mobInTurns;
    PlayerScript playerScript;
    CameraScript cameraScript;
    EffectScript effectScript;
    ScoreScript scoreScript;
    AudioScript audioScript;
    Sprite diceDefaultSprite, endTurnSprite;
    bool diceRolled = false, playerTurn = true, inCombat, stopped;
    int moveLeft, maxMove, playerIndex;

    // Start is called before the first frame update
    void Awake() {
        itemObjects = new List<ItemScriptObject>(){null, null, null};
        mobInTurns = new List<Transform>();
        diceDefaultSprite = diceSprites[6];
        endTurnSprite = diceSprites[7];

        playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
        cameraScript = GameObject.Find("/Camera").GetComponent<CameraScript>();
        effectScript = GameObject.Find("/Effect").GetComponent<EffectScript>();
        scoreScript = GameObject.Find("/Canvas/Score").GetComponent<ScoreScript>();
        audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();

        scoreScript.ToggleScore(false);
        ToggleTurn(playerTurn);
        UpdateHealth(0);

        ToggleIntro(true);
    }
    
    public void RollDice(){
        if (playerTurn && !playerScript.IsDeath()) { 
            if (maxMove == 0) { StartCoroutine(AnimateDice(10)); }
            else { EndPlayerTurn(); }

            audioScript.PlayAudio(null, "dice");
        }
    }

    public Transform GetPlatformByIndex(int index) {
        return trailObject.GetChild(index);
    }

    public void UpdateMoveCount(int count = 0){
        moveLeft = Mathf.Max(moveLeft + count, 0);
        playerTurnObject.Find("MoveCount/Text").GetComponent<Text>().text = moveLeft + " / " + maxMove;
        scoreScript.UpdateMoveMade(Mathf.Abs(count));
    }

    public void SetPlayerNextPlatform(Transform platform, bool skip = false) {
        if (platform != null && playerScript != null) {
            playerIndex = platform.GetSiblingIndex();

            if (skip) { playerScript.SetNextPost(platform); }
            else { playerScript.SetNextPlatform(platform); }
        }
    }

    public bool CheckPlatform(Transform platform) {
        int moveCount = GetMoveCount(platform),
            platformIndex = platform.GetSiblingIndex(),
            obstacleIndex = -1,
            startIndex;
        bool onRight = playerIndex > platformIndex;
        int[] trails;

        if (onRight) { startIndex = platformIndex; }
        else { startIndex = playerIndex; }

        trails = Enumerable.Range(startIndex, moveCount).ToArray();

        // check for obstacle along the way (mob etc..)
        foreach(int step in (onRight ? trails.Reverse() : trails)) {
            Transform p = trailObject.GetChild(step) ?? null,
                      obj;

            if (p != null) {
                obj = p.GetComponent<PlatformScript>().GetObject();

                if (obj != null && (obj.tag == "Mob" || obj.tag == "Monster")) {
                    obstacleIndex = step;
                    break;
                }
            }
        }

        return moveCount <= moveLeft && moveCount > 0 && obstacleIndex == -1;
    }

    public void AddMobInTurn(Transform mob) { 
        if (mobInTurns.FindIndex(m => m == mob) == -1) {
            mobInTurns.Add(mob);
        }
    }

    public void ToggleCombat(Transform other, bool isPlayer){
        StartCoroutine(AnimateCombat(other, isPlayer));
    }

    public void ShowScore(){
        scoreScript.ToggleScore(true);
        scoreScript.UpdateContent();
    }

    public void ToggleIntro(bool status){
       introObj.gameObject.SetActive(status);
       cameraScript.UpdateSize(status ? 6 : 5);
       cameraScript.SetFollow(status ? anchorObject : playerScript.transform); 
    }

    IEnumerator AnimateCombat(Transform other, bool isPlayer) {
        if (inCombat == false) {
            Vector2 playerPost = playerScript.transform.position,
                    otherPost = other.position;
            ItemScriptObject itemObj;
            float damage = 1;
            int itemIndex = -1, nextIndex;

            inCombat = true;
            anchorObject.position = (playerPost + otherPost) / 2;
            playerScript.Flip((otherPost - playerPost).normalized.x);
            ToggleCombatScreen(true);

            // player attack mob
            if (isPlayer) {
                string stance = playerScript.GetStance();
                List<ItemScriptObject> temps = itemObjects.FindAll(io => io != null && io.itemName.Equals(stance));

                if (temps.Count > 1) { 
                    for(int i = 0; i < itemObjects.Count; i++) {
                        ItemScriptObject obj = itemObjects[i] ?? null;

                        if (obj != null && obj.itemName.Equals(stance)) { itemIndex = i; }   
                    }
                }
                else { 
                    itemIndex = itemObjects.FindIndex(io => io != null && io.itemName.Equals(stance));
                }

                switch(other.tag) {
                    case "Monster":
                        MonsterScript monsterScript = other.GetComponent<MonsterScript>();
                        
                        monsterScript.Flip((playerPost - otherPost).normalized.x);

                        if (itemIndex != -1) { damage = itemObjects[itemIndex].damage; }

                        yield return new WaitForSeconds(1);
                        playerScript.AnimateAttack();
                        audioScript.PlayAudio(null, stance);

                        yield return new WaitForSeconds(.6f);
                        SpawnDamageEff(otherPost, Color.white, damage);
                        monsterScript.UpdateHealth(-damage); 
                        UpdateMoveCount(-1);
                    break;

                    case "Mob": 
                        InfectedScript infectedScript = other.GetComponent<InfectedScript>();

                        infectedScript.Flip((playerPost - otherPost).normalized.x);

                        if (itemIndex != -1) { damage = itemObjects[itemIndex].damage; }

                        yield return new WaitForSeconds(1);
                        playerScript.AnimateAttack();
                        audioScript.PlayAudio(null, stance);

                        yield return new WaitForSeconds(.6f);
                        SpawnDamageEff(otherPost, Color.white, damage);
                        infectedScript.UpdateHealth(-damage); 
                        UpdateMoveCount(-1);
                    break;
                }

                // log data for score board
                scoreScript.UpdateTotalDamage((int) damage);

                // remove weapon n equip next weapon in list (if exists)
                if (itemIndex != -1) {
                    itemObjects[itemIndex] = null; 
                    nextIndex = Mathf.Max(itemIndex - 1, 0);

                    UpdateInventoryView();

                    if (nextIndex != -1 && itemObjects[nextIndex] != null) {
                        playerScript.SetIdleStance(itemObjects[nextIndex].itemName);
                    }
                    else { playerScript.SetIdleStance("normal"); }
                }
            }
            // mob attack player
            else {
                switch(other.tag) {
                    case "Monster": 
                        MonsterScript monsterScript = other.GetComponent<MonsterScript>();

                        monsterScript.Flip((playerPost - otherPost).normalized.x);
                        damage = (int) monsterScript.GetDamage();

                        yield return new WaitForSeconds(1);
                        monsterScript.AnimateAttack();
                        audioScript.PlayAudio(null, "normal");

                        yield return new WaitForSeconds(.6f);
                        SpawnDamageEff(playerPost, Color.red, damage);
                        UpdateHealth(-damage);
                    break;
                    case "Mob": 
                        InfectedScript infectedScript = other.GetComponent<InfectedScript>();

                        itemObj = infectedScript.GetItemObject();
                        infectedScript.Flip((playerPost - otherPost).normalized.x);

                        if (itemObj != null) { damage = itemObj.damage; }

                        yield return new WaitForSeconds(1);
                        infectedScript.AnimateAttack();
                        audioScript.PlayAudio(null, itemObj != null ? itemObj.itemName : "normal");

                        yield return new WaitForSeconds(.6f);
                        SpawnDamageEff(playerPost, Color.red, damage);
                        UpdateHealth(-damage);
                    break;
                }
            }

            yield return StartCoroutine(HitStop(0.05f));
            yield return new WaitForSeconds(1);
            ToggleCombatScreen(false);
            inCombat = false;
        }
    }

    IEnumerator ExecuteEnemyTurn(){
        if (playerTurn == false) {
            GameObject[] mobs = GameObject.FindGameObjectsWithTag("Mob"), 
                         monsters = GameObject.FindGameObjectsWithTag("Monster");

            if (mobs.Length > 0 || monsters.Length > 0) {
                yield return new WaitForSeconds(.5f);

                // go through each mob
                foreach(GameObject mob in mobs) {
                    bool status = mob.GetComponent<InfectedScript>().CheckForPlayer();

                    if (status) { 
                        yield return StartCoroutine(AnimateCombat(mob.transform, false));
                    }
                }

                // go through each monster
                foreach(GameObject monster in monsters) {
                    bool status = monster.GetComponent<MonsterScript>().CheckForPlayer();

                    if (status) { 
                        yield return StartCoroutine(AnimateCombat(monster.transform, false));
                    }
                }

                yield return new WaitForSeconds(.5f);

                ToggleTurn(true);
                playerTurn = true;
                diceObject.GetComponent<Image>().sprite = diceDefaultSprite;
            }
            else { 
                ToggleTurn(true);
                playerTurn = true;
                StartCoroutine(AnimateDice(10));
            }
        }
    }

    void SpawnDamageEff(Vector2 post, Color color, float damage) {
        effectScript.StartCoroutine(effectScript.SpawnHit(post, color, "explode_1"));
        effectScript.StartCoroutine(effectScript.SpawnParticle(post, Quaternion.identity, color, 20, 360));
        effectScript.StartCoroutine(effectScript.SpawnRate(post, color, damage));
    }

    public void ToggleCombatScreen(bool status) {
        anchorObject.Find("PointLight").gameObject.SetActive(status);
        GameObject.Find("GlobalLight").GetComponent<Light2D>().intensity = status ? .2f : .5f;
        cameraScript.UpdateSize(status ? 2 : 4);
        cameraScript.SetFollow(status ? anchorObject : playerScript.transform);
    }

    public void UpdateHealth(float num) {
        Vector2 scale = healthBar.Find("Image").localScale;
        health = Mathf.Max(Mathf.Min(health + num, maxHealth), 0);

        scale.x = health / maxHealth;
        healthBar.Find("Image").localScale = scale;
        healthBar.Find("Text").GetComponent<Text>().text = health + " / " + maxHealth;

        if (health <= 0) {
            playerScript.StartCoroutine(playerScript.AnimateDeath());
        }
    }

    public int GetMoveCount(Transform platform) {
        return Mathf.Abs(playerIndex - platform.GetSiblingIndex());
    }

    public bool InCombat() { return inCombat; }

    public int GetPlayerIndex() { return playerIndex; }

    public int GetMoveLeft() { return moveLeft; }

    // get vent / hole pair
    public Transform GetPlatformPair(Transform platform) {
        Transform pair = null;

        if (platform != null) {
            string platMode = platform.GetComponent<PlatformScript>().GetMode();

            for(int i = 0; i < trailObject.childCount; i++) {
                Transform plat = trailObject.GetChild(i);
                string mode = plat.GetComponent<PlatformScript>().GetMode();

                if (platform != plat && mode.Equals(platMode)) {
                    pair = plat;
                    break;
                }
            }
        }

        return pair;
    }

    public bool MobInRange(Transform platform){
        if (platform != null) {
            ItemScriptObject itemObj = itemObjects.Find(io => 
                io != null && io.itemName.Equals(playerScript.GetStance()));
            int range = itemObj != null ? itemObj.range : 1,
                curIndex = platform.GetSiblingIndex(),
                indexDiff = Mathf.Abs(playerIndex - curIndex);

            return indexDiff <= range && moveLeft > 0;
        }
        else { return false; }

    }

    void EndPlayerTurn() {
        playerTurn = false;
        ToggleTurn(playerTurn);

        moveLeft = maxMove = 0;
        UpdateMoveCount(0);

        StartCoroutine(ExecuteEnemyTurn());
    }

    void ToggleTurn(bool isPlayerTurn) {
        Color green = new Color(0, 1, 0, .5f),
              def = new Color(1, 1, 1, .5f);

        playerTurnObject.Find("Label").GetComponent<Image>().color = isPlayerTurn ? green : def;
        enemyTurnObject.Find("Label").GetComponent<Image>().color = isPlayerTurn ? def : green;
    }

    public void PopulateInventory(ItemScriptObject itemObject){
        int newIndex = 0;

        // if inv full, remove 1st item n offset everything else
        if (itemObjects.FindAll(io => io == null).Count == 0) {
            List<ItemScriptObject> temps = new List<ItemScriptObject>(itemObjects);

            for(int i = 0; i < 3; i++) {
                if (i > 0) { temps[i - 1] = temps[i] ?? null; }
            }

            temps[temps.Count - 1] = null;
            itemObjects = temps;
        }

        for(int i = 0; i < 3; i++) {
            ItemScriptObject obj = itemObjects[i] ?? null;

            if (itemObjects[i] == null) { newIndex = i; break; }
        }

        itemObjects[newIndex] = itemObject;

        UpdateInventoryView();

        playerScript.SetIdleStance(itemObject.itemName);
    }

    void UpdateInventoryView(){
        for(int i = 0; i < 3; i++) {
            ItemScriptObject obj = itemObjects[i] ?? null;
            Image holder = inventoryObject.GetChild(i).Find("Image").GetComponent<Image>();

            if (obj != null) {
                holder.enabled = true; 
                holder.sprite = obj.sprite;
            }
            else { holder.enabled = false; }
        }
    }

    IEnumerator AnimateDice(int cycle) {
        if (diceRolled == false) {
            int tempIndex = 0;
            Image moveCountBack = playerTurnObject.Find("MoveCount").GetComponent<Image>();

            diceRolled = true;

            for(int i = 0; i < cycle; i++) {
                tempIndex = Random.Range(0, 6);
                diceObject.GetComponent<Image>().sprite = diceSprites[tempIndex];

                yield return new WaitForSeconds(0.1f);
            }

            moveLeft = maxMove = tempIndex + 1;
            UpdateMoveCount(0);

            moveCountBack.color = Color.green;

            yield return new WaitForSeconds(.8f);

            scoreScript.UpdateDiceRolled(1);
            diceObject.GetComponent<Image>().sprite = endTurnSprite;
            moveCountBack.color = new Color(1, 1, 1, .5f);
            diceRolled = false;
        }
    }

    IEnumerator HitStop(float delay) {
        if (stopped == false) {
            Time.timeScale = 0.3f;
            stopped = true;
            yield return new WaitForSecondsRealtime(delay);

            Time.timeScale = 1;
            stopped = false;
        }
    }
}
