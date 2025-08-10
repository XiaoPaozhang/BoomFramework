using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BoomFramework
{
    public class XXXLauncher : ILauncher
    {
        public void Launch()
        {
            Debug.Log($"{this.GetType().Name} 启动");
        }
    }
}
