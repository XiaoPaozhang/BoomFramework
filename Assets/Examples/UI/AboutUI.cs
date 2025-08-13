using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoomFramework
{
    public partial class AboutUI : UIBase
    {
        public override void OnOpen(object arg)
        {
            base.OnOpen(arg);

            我是按钮内容_tmptxt.text = arg as string;
        }
    }
}
