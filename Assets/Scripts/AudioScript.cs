using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioScript : MonoBehaviour
{   
    [SerializeField] AudioSource audioSource, musicSource;
    [SerializeField] List<Sprite> iconSprites;
    [SerializeField] List<AudioClip> audios;
    bool mute;

    public void PlayAudio(AudioSource source, string clipName){
        AudioClip clip = audios.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            if (source == null) { audioSource.PlayOneShot(clip); }
            else { source.PlayOneShot(clip); }
        }
    }

    public void LoopAudio(AudioSource source, string clipName){
        AudioClip clip = audios.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            if (source == null) { 
                audioSource.clip = clip;
                audioSource.Play(); 
            }
            else { 
                source.clip = clip;
                source.Play(); 
            }
        }

    }

    public void ToggleAudio(Image btn){
        mute = !mute;
        
        if (mute) { musicSource.Pause(); }
        else { musicSource.Play(); }

        btn.sprite = iconSprites[mute? 1 : 0];
    }

    public void EndLoop(AudioSource source) {
        if (source == null) { 
            audioSource.Stop();
            audioSource.clip = null;
        }
        else { 
            source.Stop(); 
            source.clip = null;
        }
    }

    public void PlayMusic(string clipName){
        AudioClip clip = audios.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { musicSource.PlayOneShot(clip); }
    }
}
