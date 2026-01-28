
using UnityEngine;

namespace BoomFramework
{
    public class FsmTest : MonoBehaviour
    {
        private TestFsm _testFsm;
        void Start()
        {
          var fsmManager = ServiceContainer.Instance.GetService<IFsmManager>();
           _testFsm = fsmManager.CreateFsm<TestFsm>(typeof(TestFsm).Name);
          _testFsm.AddState(typeof(InitState).Name, new InitState())
                  .AddState(typeof(EndState).Name, new EndState())
                  .Start(typeof(InitState).Name);
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("按下c键");
                if(_testFsm.CurrentState is InitState){
                    _testFsm.SwitchState<EndState>();
                }
                else {
                    _testFsm.SwitchState<InitState>();
                }
            }
        }
    }
}

