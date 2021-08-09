using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] float shakeDuration = 0.3f, shakeAmplitude = 1.2f, shakeFrequency = 2.0f, shakeElapsedTime = 0f;
    [SerializeField] CinemachineVirtualCamera vcam;
    CinemachineBasicMultiChannelPerlin camNoise;
    CinemachineFramingTransposer camFrame;

    // Use this for initialization
    void Start(){
        // Get Virtual Camera Noise Profile
        if (vcam != null) {
            camNoise = vcam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            camFrame = vcam.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>();
            UpdateLookAhead();
        }
    }

    // Update is called once per frame
    void Update(){
        // If the Cinemachine componet is not set, avoid update
        if (vcam != null && camNoise != null) {
            // If Camera Shake effect is still playing
            if (shakeElapsedTime > 0){
                // Set Cinemachine Camera Noise parameters
                camNoise.m_AmplitudeGain = shakeAmplitude;
                camNoise.m_FrequencyGain = shakeFrequency;

                // Update Shake Timer
                shakeElapsedTime -= Time.deltaTime;
            }
            else {
                // If Camera Shake effect is over, reset variables
                camNoise.m_AmplitudeGain = 0f;
                shakeElapsedTime = 0f;
            }
        }
    }

    public void UpdateSize(float size) {
        vcam.m_Lens.OrthographicSize = size;
    }

    public void SetFollow(Transform transform) {
        vcam.Follow = transform;
    }

    public void Shake(){
        shakeElapsedTime = shakeDuration;
    }

    public void UpdateLookAhead(float num = 0.5f){
        camFrame.m_ScreenX = num;
    }

    public void ToggleCam(bool status){
        vcam.PreviousStateIsValid = status;
    }

    public float GetLookAhead() { return camFrame.m_ScreenX; }
}
