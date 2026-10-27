using Mirror;
using UnityEngine;

public class LocalMarkerToggle : NetworkBehaviour
{
    [SerializeField] private GameObject marker;
    public override void OnStartLocalPlayer()
    {
        marker.SetActive(true);
    }

    private void Start()
    {
        if (!isLocalPlayer)
            marker.SetActive(false);
    }
}
