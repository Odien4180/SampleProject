using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillingImage;
    public TextMeshProUGUI text;

    private void Start()
    {
        Init(0, 0);
    }

    public void Init(float _current, float _max, bool _showText = false)
    {
        if (fillingImage == null) return;

        //������ ���� ���� �԰ų� �� �ʱ�ȭ �ɰ��
        if (_max == 0f)
        {
            fillingImage.fillAmount = 0f;
            text?.gameObject.SetActive(false);
            return;
        }

        fillingImage.fillAmount = _current / _max;


        text?.gameObject.SetActive(_showText);
        text?.SetText(CHString.StringBuild(_current.ToString(), "/", _max.ToString()));
    }

}
