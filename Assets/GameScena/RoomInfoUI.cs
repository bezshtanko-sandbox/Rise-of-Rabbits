
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class RoomInfoUI : MonoBehaviour
{
    public TMP_Text infoText;

    private void Start()
    {
        infoText.text = "IP: " + GetLocalIP();
    }

    private string GetLocalIP()
    {
        try
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception: " + ex.Message);
        }
        return "IP not found";
    }
}


