using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private Image[] destinationImages;
    private Text taskNumberText;
    private Text landmarkHintText;
    private int taskNumber;

    void Start()
    {
        var imgs = gameObject.GetComponentsInChildren<Image>();
        destinationImages = new Image[3];
        for (int i = 1; i < 4; ++i)
            destinationImages[i - 1] = imgs[i];
        var texts = gameObject.GetComponentsInChildren<Text>();
        taskNumberText = texts[0];
        landmarkHintText = texts[1];
    }

    public void DestinationChange(in Sprite[] sprites)
    {
        for (int i = 0; i < sprites.Length; ++i)
            destinationImages[i].sprite = sprites[i];
    }

    public void SetTaskNumber(int num)
    {
        taskNumberText.text = num.ToString();
        taskNumber = num;
    }

    public void AddTaskNumber()
    {
        taskNumberText.text = (++taskNumber).ToString();
    }

    public void SetLandmarkHint(string name)
    {
        landmarkHintText.text = name;
    }

}
