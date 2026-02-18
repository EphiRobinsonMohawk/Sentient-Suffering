using TMPro;
using UnityEngine;

public class RTUpdater : MonoBehaviour
{
    public float timer = 0;
    public RenderTexture rt;
    public TextMeshProUGUI option;
    public bool hasSwitched1 = false;
    public bool hasSwitched2 = false;
    public bool hasSwitched3 = false;
    public bool hasSwitched4 = false;
    public bool hasSwitched5 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResizeFilter(160, 90);
        option.text = "Option A";
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 8 && !hasSwitched1)
        {
            ResizeFilter(240, 135);
            hasSwitched1 = true;
            option.text = "Option B";
        }
        if (timer >= 16 && !hasSwitched2)
        {
            ResizeFilter(320, 180);
            hasSwitched2 = true;
            option.text = "Option C";

        }

        if (timer >= 24 && !hasSwitched3)
        {
            ResizeFilter(400, 225); 
            hasSwitched3 = true;
            option.text = "Option D";
        }
        if (timer >= 32 && !hasSwitched4)
        {
            ResizeFilter(480, 540); 
            hasSwitched4 = true;
            option.text = "Option E";
        }
        if (timer >= 40 && !hasSwitched5)
        {
            ResizeFilter(640, 360);
            hasSwitched5 = true;
            option.text = "Option F";
        }
    }
    
    void ResizeFilter(int w, int h)
    {
        rt.Release();   // GPU memory unplug
        rt.width = w;   // resize
        rt.height = h;
        rt.Create();
    }
}
