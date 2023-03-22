using Microsoft.Unity.VisualStudio.Editor;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class BackErrorMes
{
    public string message;
}

public class MessagesData
{
    public string role = "user";
    public string content = "";

    public MessagesData(string _content)
    {
        content = _content;
    }
}

public class Choices
{
    public MessagesData message;
    public string finish_reason;
    public int index;
}

public class ReqTextData
{
    public string model = "gpt-3.5-turbo";
    public List<MessagesData> messages = new List<MessagesData>();
    public float temperature = 0.7f;
}

public class BackTextData
{
    public string id;
    public long created;
    public string model;
    public List<Choices> choices = new List<Choices>();
}

public class ReqImageData
{
    public string prompt;
    public int n = 2;
    public string size = "1024x1024";

    public ReqImageData(string _prompt)
    {
        prompt = _prompt;
    }
}

public class BackImageData
{
    public string created;
}

public class MainScript : MonoBehaviour
{
    public InputField input_req;
    public InputField input_back;

    void Start()
    {
        
    }

    public void onClickReqText()
    {
        input_back.text = "等待返回...";

        ReqTextData reqData = new ReqTextData();
        reqData.messages.Add(new MessagesData(input_req.text));

        HttpUtil.s_instance.reqPost("https://api.openai.com/v1/chat/completions", JsonConvert.SerializeObject(reqData), (result, data) =>
        {
            if (result)
            {
                BackTextData backData = JsonConvert.DeserializeObject<BackTextData>(data);
                input_back.text = backData.choices[0].message.content;
            }
            else
            {
                input_back.text = "请求失败";
            }
        });
    }

    public void onClickReqImage()
    {
        input_back.text = "等待返回...";

        ReqImageData reqData = new ReqImageData(input_req.text);

        Debug.Log("请求数据：" + JsonConvert.SerializeObject(reqData));
        HttpUtil.s_instance.reqPost("https://api.openai.com/v1/images/generations", JsonConvert.SerializeObject(reqData), (result, data) =>
        {
            if (result)
            {
                BackImageData backData = JsonConvert.DeserializeObject<BackImageData>(data);
                input_back.text = data;
            }
            else
            {
                BackErrorMes backErrorMes = JsonConvert.DeserializeObject<BackErrorMes>(data);
                input_back.text = backErrorMes.message;
            }
        });
    }
}
