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
        baseRatio = BASE_WIDTH / BASE_HEIGHT*Screen.height;//BASE_WIDTH / BASE_HEIGHT得到一个比例，再*Screen.height得到屏幕要适配应得到的宽度
        percentScale = Screen.width / baseRatio;//当baseRatio>Screen.width，则有超出一部分看不到，这时候就需要调整了
        if (percentScale < 1)
        {
            m_transform.localScale = new Vector3(m_transform.localScale.x * percentScale, m_transform.localScale.y * percentScale, 1);//把挂该脚本的对象进行缩放
        }
    }


}
