using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnima : MonoBehaviour
{
    Text text;
    private void Start()
    {
        text = GetComponent<Text>();
    }

    public void ShowScore() //显示分数
    {
        text.text = MainGame.Instance.score.ToString();
    }
    public void ShowLife() //显示生命值
    {
        text.text = MainGame.Instance.life.ToString();
    }
}
