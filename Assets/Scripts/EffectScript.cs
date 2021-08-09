using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectScript : MonoBehaviour
{
    [SerializeField] Transform bloodPrefab, particlePrefab, ratePrefab, hitPrefab;
    Dictionary<string, List<GameObject>> objectPools;
    int maxPool = 20;

    void Awake(){
        objectPools = new Dictionary<string, List<GameObject>>();
        
        Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();

        prefabs["Particle"] = particlePrefab;
        prefabs["Blood"] = bloodPrefab;
        prefabs["Rate"] = ratePrefab;
        prefabs["Hit"] = hitPrefab;

        foreach(KeyValuePair<string, Transform> prefab in prefabs){
            string key = prefab.Key;

            objectPools[key] = new List<GameObject>();

            for(int j = 0; j < maxPool; j++){
                Transform obj = Instantiate(prefab.Value, Vector2.zero, Quaternion.identity);

                obj.parent = transform;
                obj.gameObject.SetActive(false);
                objectPools[key].Add(obj.gameObject);
            }
        }
    }

    public IEnumerator SpawnBlood(Vector2 post, Quaternion rotation, Color color, int count, float arc){
        Transform blood = GetPooledObject("Blood").transform;
        ParticleSystem system = blood.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = system.shape;
        ParticleSystem.MainModule main = system.main;
        ParticleSystem.EmissionModule emission = system.emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);

        burst.count = count;
        emission.SetBurst(0, burst);
        blood.gameObject.SetActive(true);
        blood.position = post;
        blood.rotation = rotation;
        shape.arc = arc;
        main.startColor = color;
        system.Play();

        yield return new WaitForSeconds(Random.Range(main.startLifetime.constantMin, 
            main.startLifetime.constantMax));

        blood.gameObject.SetActive(false);
    }

    public IEnumerator SpawnParticle(Vector2 post, Quaternion rotation, Color color, int count, float arc){
        Transform particle = GetPooledObject("Particle").transform;
        ParticleSystem system = particle.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = system.shape;
        ParticleSystem.MainModule main = system.main;
        ParticleSystem.EmissionModule emission = system.emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);

        burst.count = count;
        emission.SetBurst(0, burst);
        particle.gameObject.SetActive(true);
        particle.position = post;
        particle.rotation = rotation;
        shape.arc = arc;
        main.startColor = color;
        system.Play();

        yield return new WaitForSeconds(Random.Range(main.startLifetime.constantMin, 
            main.startLifetime.constantMax));

        particle.gameObject.SetActive(false);
    }

    public IEnumerator SpawnRate(Vector2 post, Color color, float num){
        Transform rate = GetPooledObject("Rate").transform;
        Text label = rate.Find("Canvas/Text").GetComponent<Text>();

        rate.gameObject.SetActive(true);
        rate.position = post;
        label.color = color;
        label.fontSize = 40;
        label.text = num.ToString(num < 1 ? ".0" : "0");

        yield return new WaitForSeconds(1);

        rate.gameObject.SetActive(false);
    }

    public IEnumerator SpawnLabel(Vector2 post, Color color, string str){
        Transform rate = GetPooledObject("Rate").transform;
        Text label = rate.Find("Canvas/Text").GetComponent<Text>();

        rate.gameObject.SetActive(true);
        rate.position = post;
        label.color = color;
        label.fontSize = 25;
        label.text = str;

        yield return new WaitForSeconds(1);

        rate.gameObject.SetActive(false);
    }

    public IEnumerator SpawnHit(Vector2 post, Color color, string anim){
        Transform hit = GetPooledObject("Hit").transform;

        hit.gameObject.SetActive(true);
        hit.position = post;
        hit.Find("Sprite").GetComponent<SpriteRenderer>().color = color;
        hit.GetComponent<Animator>().SetTrigger(anim);

        yield return new WaitForSeconds(1);

        hit.gameObject.SetActive(false);
    }



    GameObject GetPooledObject(string key){
        List<GameObject> pools = objectPools[key];
        GameObject pooledObj = null;

        // find disabled item in pool
        for(int i = 0; i < pools.Count; i++){
            if (!pools[i].activeInHierarchy) { 
                pooledObj = pools[i];
                break;
            }
        }

        // expand pool if it's not enough
        if (pooledObj == null) {
            pooledObj = Instantiate(pools[0], Vector2.zero, Quaternion.identity);
            pooledObj.transform.parent = transform;
            pooledObj.gameObject.SetActive(false);
            pools.Add(pooledObj.gameObject);
        }

        return pooledObj; 
    }
}
