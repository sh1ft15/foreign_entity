using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] Rigidbody2D rbody;
    [SerializeField] Animator animator;
    [SerializeField] int startIndex;
    GameMasterScript gameMasterScript;
    SceneLoaderScript sceneLoaderScript;
    AudioScript audioScript;
    Transform nextPlatform;
    SpriteRenderer spriteRend;
    Vector2 direction;
    Coroutine footstepCoroutine;
    List<string> stances;
    float moveSpeed = 3;
    bool facingRight = true, isDeath, clipPlayed;
    string curStance = "normal";

    void Start() {
        gameMasterScript = GameObject.Find("/GameMaster").GetComponent<GameMasterScript>();
        sceneLoaderScript = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();
        audioScript = GameObject.Find("/Audio").GetComponent<AudioScript>();
        spriteRend = transform.Find("Character/Sprite").GetComponent<SpriteRenderer>();
        stances = new List<string>{"normal", "stick", "sword", "gun", "rifle", "shotgun"};

        gameMasterScript.SetPlayerNextPlatform(gameMasterScript.GetPlatformByIndex(startIndex));
    }

    void Update() {
        if (nextPlatform != null) {
            Vector2 nextPost = nextPlatform.position + new Vector3(0, 0.6f),
                    curPost = transform.position;

            if (Vector2.Distance(nextPost, curPost) > 0.05f) {
                direction = (nextPost - curPost).normalized; 
            } 
            else { 
                direction = Vector2.zero; 
                nextPlatform = null;
                rbody.position = nextPost;
            }
        }
        else { direction = Vector2.zero; }
    }

    void FixedUpdate(){
        Move(direction.x);
    }

    public void Flip(float horizontal) {
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight)) {
            facingRight = !facingRight;
            transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
            // cameraScript.UpdateLookAhead(facingRight? 0.3f : 0.7f);
        }
    }

    public void SetNextPlatform(Transform platform) {
        nextPlatform = platform;
    }

    public void SetNextPost(Transform platform) {
        Vector2 nextPost = platform.position + new Vector3(0, 0.6f);

        rbody.position = nextPost;
        nextPlatform = platform;
    } 

    public void SetIdleStance(string stance) {
        if (stances.FindIndex(s => s.Equals(stance)) != -1) {
            foreach(string st in stances) { animator.SetBool(st, false); }

            animator.SetBool(stance, true);
            animator.Play("idle_" + stance);
            curStance = stance;
        }
    }

    public string GetStance() { return curStance; }

    public void AnimateAttack() { animator.SetTrigger("attack"); }

    void Move(float horizontal){
        float magnitude = rbody.velocity.magnitude;

        rbody.velocity = direction * moveSpeed; 
            
        Flip(horizontal);

        animator.SetFloat("magnitude", magnitude); 

        if (magnitude > 0 && footstepCoroutine == null) {
            footstepCoroutine = StartCoroutine(CycleFootStep());
        }
    }

    public bool IsDeath() { return isDeath; }

    public IEnumerator AnimateDeath() {
        if (isDeath == false) {
            spriteRend.color = Color.black;
            isDeath = true;

            yield return new WaitForSeconds(1);
            sceneLoaderScript.RestartCurrentScene();
        }
    }

    IEnumerator CycleFootStep(){
        audioScript.PlayAudio(transform.GetComponent<AudioSource>(), "walk");
        yield return new WaitForSeconds(0.3f);
        footstepCoroutine = null;
    }

    // void LoopClip(){
    //     if (clipPlayed == false) {
    //         clipPlayed = true;
    //         audioScript.LoopAudio(transform.GetComponent<AudioSource>(), "walk");
    //     }
    // }

    // void EndClipLoop(){
    //     if (clipPlayed) {
    //         clipPlayed = false;
    //         audioScript.EndLoop(transform.GetComponent<AudioSource>());
    //     }
    // }

}
