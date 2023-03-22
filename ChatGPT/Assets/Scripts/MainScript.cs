using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackErrorMes
{
    public class ErrorMes
    {
        public string message;
    }

    public ErrorMes error;
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

public class ReqTextData
{
    public string model = "gpt-3.5-turbo";
    public List<MessagesData> messages = new List<MessagesData>();
    public float temperature = 0.7f;
}

public class BackTextData
{
    public class Choices
    {
        public MessagesData message;
        public string finish_reason;
        public int index;
    }

    public string id;
    public long created;
    public string model;
    public List<Choices> choices = new List<Choices>();
}

public class ReqImageData
{
    public string prompt;
    public int n = 2;
    public string size = "512x512";

    public ReqImageData(string _prompt)
    {
        prompt = _prompt;
    }
}

public class BackImageData
{
    public class ImageData
    {
        public string url;
    }

    public string created;
    public List<ImageData> data;
}

public class MainScript : MonoBehaviour
{
    public ScrollRect scrollRectChat;
    public Transform chatListContent;
    public GameObject demo_myContent;
    public GameObject demo_gptTextContent;
    public GameObject demo_gptImageContent;

    public InputField input_req;
    public Toggle toggle_text;
    public Toggle toggle_image;

    void Start()
    {
    }

    private void Update()
    {
    }

    public void onClickReq()
    {
        if(input_req.text == "")
        {
            return;
        }

        if (toggle_text.isOn)
        {
            reqText();
        }

        if (toggle_image.isOn)
        {
            reqImage();
        }
    }

    void showMyContent()
    {
        Transform myContentTrans = Instantiate(demo_myContent, chatListContent).transform;
        myContentTrans.Find("bg/Text").GetComponent<Text>().text = input_req.text;
        LayoutRebuilder.ForceRebuildLayoutImmediate(myContentTrans.Find("bg/Text").GetComponent<RectTransform>());
        myContentTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(myContentTrans.GetComponent<RectTransform>().sizeDelta.x, myContentTrans.Find("bg/Text").GetComponent<RectTransform>().sizeDelta.y + 20);

        listToEnd();
    }

    void showGptTextContent(string message)
    {
        Transform gptContentTrans = Instantiate(demo_gptTextContent, chatListContent).transform;
        Text text = gptContentTrans.Find("bg/Text").GetComponent<Text>();

        while (message[0] == '\n')
        {
            message = message.Substring(1);
        }
        text.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(text.GetComponent<RectTransform>());
        gptContentTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(gptContentTrans.GetComponent<RectTransform>().sizeDelta.x, text.GetComponent<RectTransform>().sizeDelta.y + 20);

        listToEnd();
    }

    void showGptImageContent(string url)
    {
        Transform gptContentTrans = Instantiate(demo_gptImageContent, chatListContent).transform;
        gptContentTrans.GetComponent<NetImageScript>().load(url);
        
        listToEnd();
    }

    void listToEnd()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatListContent.GetComponent<RectTransform>());
        if(chatListContent.GetComponent<RectTransform>().sizeDelta.y <= 1000)
        {
            return;
        }
        scrollRectChat.movementType = ScrollRect.MovementType.Clamped;
        chatListContent.localPosition = new Vector3(0,chatListContent.GetComponent<RectTransform>().sizeDelta.y - 1000,0);
        scrollRectChat.movementType = ScrollRect.MovementType.Elastic;
    }

    void reqText()
    {
        showMyContent();

        ReqTextData reqData = new ReqTextData();
        reqData.messages.Add(new MessagesData(input_req.text));

        HttpUtil.s_instance.reqPost("https://api.openai.com/v1/chat/completions", JsonConvert.SerializeObject(reqData), (result, data) =>
        {
            input_req.text = "";

            if (result)
            {
                BackTextData backData = JsonConvert.DeserializeObject<BackTextData>(data);
                string message = backData.choices[0].message.content;
                showGptTextContent(message);
            }
            else
            {
                showGptTextContent("请求失败");
            }
        });

        input_req.text = "等待返回...";
    }

    void reqImage()
    {
        showMyContent();

        ReqImageData reqData = new ReqImageData(input_req.text);

        Debug.Log("请求数据：" + JsonConvert.SerializeObject(reqData));
        HttpUtil.s_instance.reqPost("https://api.openai.com/v1/images/generations", JsonConvert.SerializeObject(reqData), (result, data) =>
        {
            input_req.text = "";

            if (result)
            {
                //Debug.Log(data);
                BackImageData backData = JsonConvert.DeserializeObject<BackImageData>(data);
                for(int i = 0; i < backData.data.Count; i++)
                {
                    showGptImageContent(backData.data[i].url);
                }
            }
            else
            {
                BackErrorMes backErrorMes = JsonConvert.DeserializeObject<BackErrorMes>(data);
                showGptTextContent(backErrorMes.error.message);
            }
        });

        input_req.text = "等待返回...";
    }
}
