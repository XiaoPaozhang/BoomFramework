
using System.Collections;
using UnityEngine;

namespace BoomFramework
{
    public class UITest : MonoBehaviour
    {
        private IUIManager _uIManager;
        [SerializeField]
        private int delayTime = 2;
        void Start()
        {
            _uIManager = ServiceContainer.Instance.GetService<IUIManager>();
            _uIManager.RegisterUI<HomeUI>();
            _uIManager.RegisterUI<AboutUI>();
            HomeUI HomeUI = _uIManager.OpenUI<HomeUI>("你好世界HomeUI");
            AboutUI AboutUI = _uIManager.OpenUI<AboutUI>("你好世界AboutUI");
            StartCoroutine(BackUI());
        }

        void Update()
        {

        }

        private IEnumerator BackUI()
        {
            yield return new WaitForSeconds(delayTime);
            _uIManager.CloseUI<HomeUI>();
            yield return new WaitForSeconds(delayTime);
            _uIManager.CloseUI<AboutUI>();
            yield return new WaitForSeconds(delayTime);
            _uIManager.OpenUI<HomeUI>("你好世界HomeUI");

            // 测试关闭未打开的ui时的警告输出情况
            _uIManager.CloseUI<AboutUI>();
        }
    }

}

