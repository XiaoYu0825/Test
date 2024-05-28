using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageType
{
    public struct Game
    {
       
    }
    public struct NetConnected
    {
        public ServerEnum serverEnum;
    }

    public struct NetDisconnected
    {
        public ServerEnum serverEnum;
    }
}
