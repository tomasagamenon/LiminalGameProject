using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : Interactable
{
    private bool ligth;
    [SerializeField]
    private Material ligthOn;
    [SerializeField]
    private Material lightOff;
    [SerializeField]
    private List<GameObject> lightsRoof;
    [SerializeField]
    private List<GameObject> lights;
    [SerializeField]
    private List<GameObject> darkness;

    public override void Interact()
    {
        StopAllCoroutines();
        if (ligth)
            ligth = false;
        else ligth = true;
        int numOfLight = 0;
        foreach (GameObject light in lightsRoof)
        {
            if (ligth == true)
            {
                StartCoroutine(Flashing(true, light, numOfLight));
            }
            else
            {
                StartCoroutine(Flashing(false, light, numOfLight));
            }
            numOfLight++;
        }
    }

    IEnumerator Flashing(bool a, GameObject light, int numOfLight)
    {
        var b = Random.Range(1, 10);
        for (int i = 0; i<= b; i++)
        {
            float c = Random.Range(0.01f, 0.5f);
            if (lights[numOfLight].activeSelf == true)
            {
                light.GetComponentInChildren<Renderer>().material = lightOff;
                lights[numOfLight].SetActive(false);
                if (darkness.Count > numOfLight)
                    darkness[numOfLight].SetActive(true);
            }
            else
            {
                light.GetComponentInChildren<Renderer>().material = ligthOn;
                lights[numOfLight].SetActive(true);
                if (darkness.Count > numOfLight)
                    darkness[numOfLight].SetActive(false);
            }
            yield return new WaitForSeconds(c);
        }
        if (!a)
        {
            light.GetComponentInChildren<Renderer>().material = lightOff;
            lights[numOfLight].SetActive(false);
            if (darkness.Count > numOfLight)
                darkness[numOfLight].SetActive(true);
        }
        else
        {
            light.GetComponentInChildren<Renderer>().material = ligthOn;
            lights[numOfLight].SetActive(true);
            if (darkness.Count > numOfLight)
                darkness[numOfLight].SetActive(false);
        }
    }
}
