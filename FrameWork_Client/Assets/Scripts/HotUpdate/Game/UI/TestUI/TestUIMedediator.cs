using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUIMedediator : UIMediator<TestUIView>
{
    protected override void OnInit(TestUIView view)
    {
        base.OnInit(view);
    }
    protected override void OnShow(object arg)
    {
        base.OnShow(arg);

        this.eventTable.ListenEvent("TestButton", OnTestButton);
    }

    private void OnTestButton(object[] args)
    {
        Debug.Log("°´Å¥µã»÷");
    }
}
