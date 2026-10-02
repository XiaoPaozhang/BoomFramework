
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BoomFramework
{
    public abstract class LauncherBase : MonoBehaviour, ILauncher
    {
        public abstract IEnumerator Launch();
    }
}