using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerTD : NetworkManager
{
    public override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = 60;
    }

}
