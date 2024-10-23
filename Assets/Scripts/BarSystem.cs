using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarSystem : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] bool shake;
    [SerializeField] bool isHidden;
    [SerializeField] float displayTime;
    [SerializeField] float fadeTime;

    float t = 0f;
    bool startedFade = false;

    [SerializeField] List<Image> imagesToDisplay;

    [SerializeField] Animator animator;

    public void SetValue(float barValue, float valueRemoved)
    {
        rectTransform.localScale = new Vector3(barValue, 1f, 1f);
        if (shake) animator.Play("Shake", 0, 0f);
        if (isHidden)
        {
            if(startedFade)
            {
                if (t > fadeTime && t < displayTime - fadeTime)
                    t = fadeTime;
                else if (t > displayTime - fadeTime)
                    t = displayTime - t;
                return;
            }
            
            startedFade = true;
            StartCoroutine(DisplayImages());
        }
    }

    void Start()
    {
        imagesToDisplay.ForEach(j => 
        { 
            SetImageTransparency(j, isHidden ? 0f: 1f);
        });
    }

    IEnumerator DisplayImages()
    {
        t = 0f;

        while (t < displayTime)
        {
            t += Time.deltaTime;

            if (t < fadeTime)
            {
                foreach (Image image in imagesToDisplay)
                {
                    SetImageTransparency(image, t / fadeTime);
                }
            }
            else if(t > displayTime - fadeTime)
            {
                foreach (Image image in imagesToDisplay)
                {
                    SetImageTransparency(image, (displayTime - t) / fadeTime);
                }
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }

        startedFade = false;
    }

    private void SetImageTransparency(Image image, float transparency)
    {
        Color color = image.color;
        color.a = transparency;
        image.color = color;
    }
}
