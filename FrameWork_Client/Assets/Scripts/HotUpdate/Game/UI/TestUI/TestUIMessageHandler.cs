using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestUIMessageHandler : MessageHandler<MessageType.Test>
{
    public override async Task HandleMessage(MessageType.Test arg)
    {
        await Task.Yield();
    }
}
