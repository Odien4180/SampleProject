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
            // 스크린 좌표를 월드 좌표로 변환함
            Vector3 wp = particleCamera.ScreenToWorldPoint(Input.mousePosition);
            
            Vector2 touchPos = new Vector2(wp.x, wp.y);

            // 오브젝트의 위치를 갱신함
            particle.transform.position = touchPos;
        }
    }
}