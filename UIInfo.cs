using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfo
{
    private string file1;
    private string body1;
    private string body2;

    public string File1
    {
        get { return file1; }
        set { file1 = value; }
    }

    public string Body1
    {
        get { return body1; }
        set { body1 = value; }
    }

    public string Body2
    {
        get { return body2; }
        set { body2 = value; }
    }
    //拼接字符串
    public UIInfo(string name, string contrastKey, string path)
    {
        string path2 = path.Substring(1);
        file1 = string.Format("     public {0} {1}", GetUIPath.typMap[contrastKey], name);
        body1 = string.Format("     {0} =transform.Find(\"{1}\").GetComponent<{2}>()", name, path2, GetUIPath.typMap[contrastKey]);
        body2 = string.Format("     {0}=null", name);
    }

}