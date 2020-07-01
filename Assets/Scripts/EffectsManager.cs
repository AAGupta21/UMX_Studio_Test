using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField] private Camera cam = null;
    [SerializeField] private float CameraShake_Offset = 0.25f;
    public Color DefaultColor = Color.black;
    [SerializeField] private Color[] ColorArray = null;
    
    public void CameraShake(float Duration)
    {
        StartCoroutine(CameraShakeMotion(Duration));
    }

    private IEnumerator CameraShakeMotion(float CameraShake_Duration)
    {
        float currTime = 0f;
        Vector3 oriPos = cam.transform.position;

        while(currTime < CameraShake_Duration)
        {
            cam.transform.position = oriPos + Random.insideUnitSphere * CameraShake_Offset;

            yield return new WaitForSeconds(0.03f);
            currTime += 0.02f;
        }

        cam.transform.position = oriPos;
    }

    public void ChangeBgColor(int PlayerIndex)
    {
        if(PlayerIndex >= 0)
        {
            cam.backgroundColor = ColorArray[PlayerIndex];
        }
        else
        {
            cam.backgroundColor = DefaultColor;
        }
    }
}
