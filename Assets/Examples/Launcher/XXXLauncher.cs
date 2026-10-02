using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BoomFramework
{
    public class XXXLauncher : LauncherBase
    {
        public override IEnumerator Launch()
        {
            Debug.Log($"{this.GetType().Name} 启动");
            yield return null;
        }
    }
}
