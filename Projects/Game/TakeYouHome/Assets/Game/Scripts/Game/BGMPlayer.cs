using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public AudioClip[] BGMs;

    private bool[] isPlayed;
    private AudioSource audioSource;

    private int currentPlayedIndex;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        isPlayed = new bool[BGMs.Length];
        for (int i = 0; i < BGMs.Length; ++i)
            isPlayed[i] = false;
        StartCoroutine(BGMPlaying());
    }

    private IEnumerator BGMPlaying()
    {
        while (true)
        {
            if (audioSource.isPlaying == false)
            {
                audioSource.clip = BGMs[PickNextIndex()];
                audioSource.Play();
            }
            yield return new WaitForSeconds(10);
        }
    }

    private int PickNextIndex()
    {
        int len = BGMs.Length;
        int r = currentPlayedIndex;
        if (r == -1) r = Random.Range(0, len);
        int i = r;
        do
        {
            if (isPlayed[i] == false)
            {
                isPlayed[i] = true;
                r = i;
                currentPlayedIndex = r;
                break;
            }
            i = (i + 1) % len;
            if (i == r)
            {
                for (int j = 0; j < BGMs.Length; ++j)
                    isPlayed[j] = false;
            }
        }
        while (true);

        return r;
    }
}
