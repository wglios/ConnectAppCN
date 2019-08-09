using System;
using System.Collections.Generic;
using ConnectApp.Constants;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.Plugins {
    public class WechatPlugin {
        public static WechatPlugin instance(Action<string> codeCallBack = null) {
            if (plugin == null) {
                plugin = new WechatPlugin();
            }

            if (codeCallBack != null) {
                plugin.codeCallBack = codeCallBack;
            }

            return plugin;
        }

        public static WechatPlugin plugin;

        bool isListen;

        public BuildContext context;

        public Action<string> codeCallBack;

        public void addListener() {
            if (!this.isListen) {
                UIWidgetsMessageManager.instance.AddChannelMessageDelegate("wechat", this._handleMethodCall);
                this.isListen = true;
            }
        }

        void _handleMethodCall(string method, List<JSONNode> args) {
            using (WindowProvider.of(this.context).getScope()) {
                switch (method) {
                    case "callback": {
                        var node = args[0];
                        var dict = JSON.Parse(node);
                        var type = dict["type"];
                        if (type == "code") {
                            var code = dict["code"];
                            if (this.codeCallBack != null) {
                                this.codeCallBack(code);
                            }
                        }
                    }
                        break;
                }
            }
        }


        public void login(string stateId) {
            if (!Application.isEditor) {
                this.addListener();
                loginWechat(stateId);
            }
        }

        public void shareToFriend(string title, string description, string url, string imageBytes) {
            if (!Application.isEditor) {
                this.addListener();
                toFriends(title, description, url, imageBytes);
            }
        }

        public void shareToTimeline(string title, string description, string url, string imageBytes) {
            if (!Application.isEditor) {
                this.addListener();
                toTimeline(title, description, url, imageBytes);
            }
        }

        public void shareToMiniProgram(string title, string description, string url, string imageBytes, string path) {
            if (!Application.isEditor) {
                this.addListener();
                toMiNiProgram(title, description, url, imageBytes, Config.MINIID, path);
            }
        }


        public bool isInstalled() {
            if (!Application.isEditor) {
                this.addListener();
                return isInstallWechat();
            }

            return false;
        }

        public void toOpenMiNi(string path) {
            if (!Application.isEditor) {
                this.addListener();
                openMiNi(Config.MINIID, path);
            }
        }
#if UNITY_IOS
        [DllImport("__Internal")]
        internal static extern void loginWechat(string stateId);

        [DllImport("__Internal")]
        internal static extern bool isInstallWechat();

        [DllImport("__Internal")]
        internal static extern void toFriends(string title, string description, string url, string imageBytes);

        [DllImport("__Internal")]
        internal static extern void toTimeline(string title, string description, string url, string imageBytes);

        [DllImport("__Internal")]
        internal static extern void toMiNiProgram(string title, string description, string url, string imageBytes,
            string ysId, string path);

        [DllImport("__Internal")]
        internal static extern void openMiNi(string ysId, string path);

#elif UNITY_ANDROID
        static AndroidJavaObject _plugin;

        static AndroidJavaObject Plugin() {
            if (_plugin == null) {
                using (
                    AndroidJavaClass managerClass =
                        new AndroidJavaClass("com.unity3d.unityconnect.plugins.WechatPlugin")
                ) {
                    _plugin = managerClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            return _plugin;
        }

        static void loginWechat(string stateId) {
            Plugin().Call("loginWechat", stateId);
        }

        static bool isInstallWechat() {
            return Plugin().Call<bool>("isInstallWechat");
        }

        static void toFriends(string title, string description, string url, string imageBytes) {
            Plugin().Call("shareToFriends", title, description, url, imageBytes);
        }

        static void toTimeline(string title, string description, string url, string imageBytes) {
            Plugin().Call("shareToTimeline", title, description, url, imageBytes);
        }

        static void toMiNiProgram(string title, string description, string url, string imageBytes, string ysId,
            string path) {
            Plugin().Call("shareToMiNiProgram", title, description, url, imageBytes, ysId, path);
        }

        static void openMiNi(string ysId, string path) {
            Plugin().Call("openMiNi", Config.wechatAppId, ysId, path);
        }
#else
        static bool isInstallWechat() {return true;}
        static void loginWechat(string stateId) {}
        static void toFriends(string title, string description, string url,string imageBytes) {}
        static void toTimeline(string title, string description, string url,string imageBytes) {}
        static void toMiNiProgram(string title, string description, string url, string imageBytes,string ysId, string path) {}
        static void openMiNi(string ysId, string path) {}
#endif
    }
}