using UnityEngine;

public class Blip : MonoBehaviour
{
    public enum MediaSource
    {
        Face,
        Inst,
        Twit,
    }

    public int ID;
    public int CountdownNumber;
    public string Message;
    public MediaSource Source;
    //Reference this game object for transform

    public BarData.Bar data;

    public int lockPos = -1;

    public float color_percent;

}
