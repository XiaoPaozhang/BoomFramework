using System.Collections.Generic;
using System.Collections;
using System;

namespace BoomFramework
{
    public interface IUIManager
    {
        bool IsInit { get; }
        void Init(string uiLoadPath, UIRootContext uIRootContext, IAssetLoadManager assetLoadManager, IObjectPoolManager objectPoolManager);
        /// <summary>
        /// 注册期确定 ui 名称与默认层。参数可选，默认：uiName=typeof(T).Name，uILayer=UILayer.Page
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uiName"></param>
        /// <param name="uILayer"></param>
        void RegisterUI<T>(string uiName = null, UILayer uILayer = UILayer.Page) where T : UIBase;
        T GetUI<T>() where T : UIBase;
        /// <summary>
        /// 按注册好的默认层打开（不再传层）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        T OpenUI<T>(object arg = null) where T : UIBase;
        /// <summary>
        /// 关闭该类型的顶层活跃实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void CloseUI<T>() where T : UIBase;
        /// <summary>
        /// 关闭所有活跃的 UI 实例
        /// </summary>
        void CloseAll();
        /// <summary>
        /// 已废弃：当前未维护页面栈，Back 不生效，请改用 CloseUI<T>() 或 CloseAll()。
        /// </summary>
        [Obsolete("当前未维护页面栈，Back 不生效，请改用 CloseUI<T>() 或 CloseAll().", false)]
        bool Back();
        void UnInit();
    }
}
