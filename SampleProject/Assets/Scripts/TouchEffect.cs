using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchEffect : Singleton<TouchEffect>
{
    public Camera particleCamera;
    public ParticleSystem particle;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ��
            Vector3 wp = particleCamera.ScreenToWorldPoint(Input.mousePosition);
            
            Vector2 touchPos = new Vector2(wp.x, wp.y);

            // ������Ʈ�� ��ġ�� ������
            particle.transform.position = touchPos;
        }
    }
}