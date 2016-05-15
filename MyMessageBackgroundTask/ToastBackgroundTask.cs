﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace MyMessageBackgroundTask
{
    public sealed class ToastBackgroundTask : IBackgroundTask
    {
        private ApplicationDataContainer appSetting = ApplicationData.Current.LocalSettings; //本地存储
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            if (bool.Parse(appSetting.Values["isUseingBackgroundTask"].ToString()))
            {
                ToastNotificationActionTriggerDetail details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                if (details != null)
                {
                    // 是否选择“确定”
                    string arg = details.Argument;
                    if (arg.Substring(0, 2) == "ok")
                    {
                        // 获取数据
                        string value = details.UserInput["content"] as string;
                        Debug.WriteLine(arg.Substring(2));
                        if (value != "")
                        {
                            List<KeyValuePair<String, String>> paramList = new List<KeyValuePair<String, String>>();
                            paramList.Add(new KeyValuePair<string, string>("article_id", arg.Substring(2)));
                            paramList.Add(new KeyValuePair<string, string>("type_id", "5"));
                            paramList.Add(new KeyValuePair<string, string>("stuNum", appSetting.Values["stuNum"].ToString()));
                            paramList.Add(new KeyValuePair<string, string>("idNum", appSetting.Values["idNum"].ToString()));
                            paramList.Add(new KeyValuePair<string, string>("content", value));
                            string sendMark = await NetWork.getHttpWebRequest("cyxbsMobile/index.php/Home/ArticleRemark/postremarks", paramList);
                            Debug.WriteLine(sendMark);
                            try
                            {
                                if (sendMark != "")
                                {
                                    JObject obj = JObject.Parse(sendMark);
                                    if (Int32.Parse(obj["state"].ToString()) == 200)
                                    {
                                        Utils.Toast("评论成功");
                                    }
                                    else
                                    {
                                        Utils.Toast("评论失败");
                                    }
                                }
                                else
                                {
                                    Utils.Toast("评论失败");
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    deferral.Complete();
                }
            }
        }
    }
}
