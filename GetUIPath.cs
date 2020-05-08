using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
//using System.Windows.Forms;
using Application = UnityEngine.Application;
using Button = UnityEngine.UI.Button;
using MenuItem = UnityEditor.MenuItem;
using System.IO;
using System.Text;



//#region

/* 注意事项：  UI命名规则
 Button: but_xxx;
 Text: txt_xxx;
 Image: img_xxx;
 */
public class GetUIPath : Editor
{
    //写文件Stream
    static StreamWriter writer;
    //读文件Stream
    static StreamReader reader;
    //获取到的点击物体名称
    static string nameStr = "";
    //创建字典
    public static Dictionary<string, string> typMap = new Dictionary<string, string>()
    {
        //定义物体命名的规范
        {"btn",typeof(Button).Name },
        {"txt",typeof(Text).Name },
        {"img",typeof(Image).Name }
    };

    private static List<UIInfo> uinfo = new List<UIInfo>();

    //没现有脚本时创建时调用的string
    private static string Eg_str1 =
        "using System.Collections;" +
        "\r\nusing System.Collections.Generic;" +
        "\r\nusing UnityEngine;\r\nusing UnityEngine.UI;" +
        "\r\n//2020-03-21完成\r\n" +
        "public class @Name : MonoBehaviour {\r\n\r\n\t" +
        "//Use this for initialization\r\n\tvoid Start () \r\n\t{\r\n\t     OnAutoLoadedUIObj();\r\n\t}\r\n\t\r\n\t" +
        "//Update is called once per frame\r\n\tvoid Update () {\r\n\t\t\r\n\t}\r\n" +
        "\t#region\r\n\t" +
        "public  void OnAutoLoadedUIObj()\r\n\t{\r" +
        "@Body1\r\n\t}\r\n\t" +
        "public  void OnAutoRelease()\r\n\t{\r" +
        "@Body2\r\n\t}\r\n" +
        "@File1\r\n\t#endregion\r\n}";
    //有脚本时时调用的string
    private static string Eg_str2 = 
        "\t#region\r\n\t"+
        "public  void OnAutoLoadedUIObj()\r\n\t{\r" +
        "@Body1\r\n\t}\r\n\t" +
        "public  void OnAutoRelease()\r\n\t{\r"+
        "@Body2\r\n\t}\r\n" +
        "@File1\r\n\t#endregion\r\n}";
    //生成定义的string
    private static string File1 = "";
    //生成获取Find的string
    private static string Body1 = "";
    //生成对象=NULL的string
    private static string Body2 = "";
    //写入或创建脚本
    static void WriteScript()
    {
        //字符串生成
        for (int i = 0; i < uinfo.Count; i++)
        {
            File1 += uinfo[i].File1 + ";\r\n";
            Body1 += uinfo[i].Body1 + ";\r\n";
            Body2 += uinfo[i].Body2 + ";\r\n";
            //Debug.Log(uinfo[i].File1 + "_____________" + uinfo[i].Body1 + "______________" + uinfo[i].Body2);
        }

        //Debug.Log(File1 + "---------------" + Body1);
        //储存文档
        FileInfo file = new FileInfo(Application.dataPath + "/UnityUI/" + nameStr + ".cs");
        if (file.Exists)
        {
            //替换字符
            Eg_str2 = Eg_str2.Replace("@File1", File1);
            Eg_str2 = Eg_str2.Replace("@Body1", Body1);
            Eg_str2 = Eg_str2.Replace("@Body2", Body2);
            //debug.LogError("路径：" + Application.dataPath + "/sence0/TestName.cs");

            //file.Delete();
            // file.Refresh();
            //写入脚本
            reader = new StreamReader(Application.dataPath + "/UnityUI/"+ nameStr + ".cs", Encoding.UTF8);
            string text;
            string ReadTxt = "";
            while ((text = reader.ReadLine()) != null && text.IndexOf("#region")==-1/*text.Equals("#region")!=true*/)
            {
                ReadTxt += text+"\r\n";
                Debug.LogWarning(text+"-----"+ text.IndexOf("#region"));
            }
            reader.Dispose();
            reader.Close();
            writer = file.CreateText();
            writer.WriteLine(ReadTxt+Eg_str2);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }
        else
        {
            //创建并写入脚本
            Eg_str1 = Eg_str1.Replace("@File1", File1);
            Eg_str1 = Eg_str1.Replace("@Body1", Body1);
            Eg_str1 = Eg_str1.Replace("@Body2", Body2);
            Eg_str1 = Eg_str1.Replace("@Name", nameStr);
            writer = file.CreateText();
            writer.WriteLine(Eg_str1);
            writer.Flush();
            writer.Dispose();
            writer.Close();
            //SaveFileDialog saveFile = new SaveFileDialog();
            //saveFile.FileName = "TestName.cs";
            //string path = Environment.CurrentDirectory.Replace("/", @"\");
            ////Debug.LogError("这是什么：" + path);
            //if (saveFile.ShowDialog() == DialogResult.OK)
            //{
            //    string[] name = saveFile.FileName.Split('\\');
            //    string nameStr = name[name.Length - 1].Replace(".cs", "");
            //    Eg_str1 = Eg_str1.Replace("@Name", nameStr);
            //    File.WriteAllText(saveFile.FileName, Eg_str1);
            //}
        }


    }
    /// <summary>
    /// 获取物体的路径
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    static string GetgameObjectPath(Transform go)
    {
       // Debug.Log("正在获取路径····");
        string path = "";
       // Debug.Log(go.name);
       // Debug.Log(Selection.gameObjects[0].name);
        while (go != Selection.gameObjects[0]&& go.name!= "Canvas")
        {
            //Selection.gameObjects[0]
           // Debug.LogWarning(go.name+" -------  "+ Selection.gameObjects[0].name);
            path = path.Insert(0, go.name);
            path = path.Insert(0, "/");
            go = go.parent;
           // Debug.Log(path);
        }
       // Debug.LogWarning(path);
        return path;
    }
    /// <summary>
    /// 获取物体的基本信息(名字，校准key，路径)，并储存
    /// </summary>
    /// <param name="tf"></param>
    static void GetChildinfo(Transform tf)
    {
       // Debug.Log(tf.name);

        foreach (Transform tfChild in tf)
        {
            try
            {
                //物体名称长度小于4的不获取
                if(tfChild.name.Length<4)
                {
                    Debug.LogError("物体名字长度小于4");
                }
                else
                {
                    //获取前三个字符
                    string contrastKey = tfChild.name.Substring(0, 3);
                    //Debug.LogWarning("获取到字符" + contrastKey);
                    //判定是否存在命名规范
                    if (typMap.ContainsKey(contrastKey))
                    {
                        Debug.Log(tfChild.name + "------" + contrastKey + "------" + GetgameObjectPath(tfChild));
                        UIInfo uinf = new UIInfo(tfChild.name, contrastKey, GetgameObjectPath(tfChild));
                        uinfo.Add(uinf);
                    }
                    if (tfChild.childCount >= 0)
                    {
                        GetChildinfo(tfChild);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("错误········"+ tfChild.name+ e.Message);
            }
        }
    }
    //显示在Unity3d编辑器中
    [MenuItem("MyTools/GetScript")]
    static void CreatScript()
    {
        //获取点击到的物体以及以下的子物体
        GameObject[] select = Selection.gameObjects;
        
        Debug.Log(Selection.gameObjects);
        if (select.Length == 1)
        {
            nameStr = Selection.gameObjects[0].name;
            Transform selectGo = select[0].transform;
            GetChildinfo(selectGo);
            WriteScript();
        }
        else
        {
            EditorUtility.DisplayDialog("警告", "你只能选择一个GameObject", "确定");
        }

    }
}