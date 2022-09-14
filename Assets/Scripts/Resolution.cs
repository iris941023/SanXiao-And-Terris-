using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolution : MonoBehaviour
{
    // Start is called before the first frame update
    public float BASE_WIDTH = 1920;
    public float BASE_HEIGHT = 1280F;
    private Transform m_transform;
    private float baseRatio;
    private float percentScale;
    void Start()
    {
        m_transform = transform;
        seScale();
    }
    void seScale()
    {
        baseRatio = BASE_WIDTH / BASE_HEIGHT*Screen.height;//BASE_WIDTH / BASE_HEIGHT�õ�һ����������*Screen.height�õ���ĻҪ����Ӧ�õ��Ŀ��
        percentScale = Screen.width / baseRatio;//��baseRatio>Screen.width�����г���һ���ֿ���������ʱ�����Ҫ������
        if (percentScale < 1)
        {
            m_transform.localScale = new Vector3(m_transform.localScale.x * percentScale, m_transform.localScale.y * percentScale, 1);//�ѹҸýű��Ķ����������
        }
    }


}
