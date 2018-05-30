# EamUnityEditorTool__LuaPreview
EamUnityEditorTool--LuaPreview&lt;Unity编辑器工具--文本文件预览>

CSDN博客地址:[点击这里](https://blog.csdn.net/qq_33337811/article/details/77099001)

击Project里一个C#脚本，在Inspector面板就会出现脚本内容的预览，但是项目中的lua文件点击了却不会这样显示。

所以，我们点击资源时，更具资源的后缀名来选择显示在Inspector面板的内容，例如，lua文件点击了可以直接把文本内容显示在Inspector面板。

效果：(点击的是lua文件，为了保密，内容替换为一组任意文字)
![](https://i.imgur.com/5bkutB5.png)

代码：

因为资源文件可能有很多种后缀名，不同的可能需求也不一样，所以我们写个属性类，不同需求的使用不同的属性：

    public class CustomAssetAttribute : Attribute {
    
	    public string[] extensions; //不同属性对应不同需求，同一需求可能有几种类型文件
	    
	    public CustomAssetAttribute(params string[] extention)
	    {
	    this.extensions = extention;
	    }
    }

然后以lua文件这种文本资源为例，面板绘制：

    /// <summary>
/// lua文件绘制方式
/// </summary>
[CustomAsset(".lua")]  //绘制方式绑定以.lua为后缀的资源文件
public class LuaInspector : Editor
{
    private string content;
    private bool show = false;
    private string showKey = "LuaInspectorShown";

    void OnEnable()
    {
        string path = Application.dataPath + "/" + AssetDatabase.GetAssetPath(Selection.activeObject).Substring(7);

        try
        {
            TextReader tr = new StreamReader(path);
            content = tr.ReadToEnd(); //读取文件中所有文本
            tr.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public override void OnInspectorGUI()
    {
        show = EditorPrefs.GetBool("showKey");
        show = GUILayout.Toggle(show, "Show Lua Content");
        EditorPrefs.SetBool("showKey", show);
        if (show) //这里为了性能，加了开关
        {
            GUILayout.Label(content);
        }
    }
	}

目前可见这两个文件关联性不大，第二个脚本设置了绘制方式，但是还没法使以.lua后缀的文件就以这种方式绘制了，于是有了：

    [CustomEditor(typeof(DefaultAsset))] //自定义--资源的Inspector绘制方式
public class DefaultAssetInspector : Editor
{
    private Editor editor; //资源文件预览绘制方式
    private static Type[] customAssetTypes;

    [InitializeOnLoadMethod] //Unity软件启动事件
    static void Init()
    {
        customAssetTypes = GetCustomAssetTypes();
    }

    /// <summary>
    /// 获取所有带自定义属性CustomAssetAttribute的类
    /// </summary>
    private static Type[] GetCustomAssetTypes()
    {
        var assemblyPaths = Directory.GetFiles("Library/ScriptAssemblies", "*.dll");
        var types = new List<Type>();
        var customAssetTypes = new List<Type>();

        foreach (var assembly in assemblyPaths
            .Select(assemblyPath => Assembly.LoadFile(assemblyPath)))
        {
            types.AddRange(assembly.GetTypes());
        }
        //到这一步，所有的类都在types这个列表里
        foreach (var type in types)
        {
            //在每个类中尝试寻找使用自定义属性CustomAssetAttribute
            var customAttributes =
                type.GetCustomAttributes(typeof(CustomAssetAttribute), false) //获取自定义属性CustomAssetAttribute
                                                      as CustomAssetAttribute[];

            if (0 < customAttributes.Length)
                customAssetTypes.Add(type); //找到了就把这个类存到customAssetTypes列表中
        }

        //so返回的是所有程序集中找到的带CustomAssetAttribute属性的类的数组(如前面的LuaInspector类)
        return customAssetTypes.ToArray();
    }

    /// <summary>
    /// 根据扩展名去含指定属性的类数组中获取对应类(对比属性的extensions值)
    /// </summary>
    private Type GetCustomAssetEditorType(string extension)
    {
        foreach (var type in customAssetTypes)
        {
            var customAttributes =
              type.GetCustomAttributes(typeof(CustomAssetAttribute), false)
                                                      as CustomAssetAttribute[];

            foreach (var customAttribute in customAttributes)
            {
                if (customAttribute.extensions.Contains(extension))
                    return type;
            }
        }
        return typeof(DefaultAsset);
    }

    private void OnEnable()
    {
        var assetPath = AssetDatabase.GetAssetPath(target);

        var extension = Path.GetExtension(assetPath); //获取扩展名
        //根据扩展名去得对应的继承自Editor的绘制方式
        var customAssetEditorType = GetCustomAssetEditorType(extension);
        //使目标以指定绘制方式绘制
        editor = CreateEditor(target, customAssetEditorType);
    }

    //先去程序集中遍历寻找带有CustomAssetAttribute这个自定义的属性的类，找到这些个类后,就得到了后缀名和对应绘制方式
    //（因为这个属性关联了后缀名和绘制方式）
    //所以我们去用现在点击的资源的后缀名去获取对应的绘制方式，用这种方式绘制Inspector面板

    public override void OnInspectorGUI()
    {
        if (editor != null)
        {
            GUI.enabled = true;
            editor.OnInspectorGUI();
        }
    }

    public override bool HasPreviewGUI() //是否有Inspector面板下面的预览小界面
    {
        return editor != null ? editor.HasPreviewGUI() : base.HasPreviewGUI();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) //小界面内容
    {
        if (editor != null)
            editor.OnPreviewGUI(r, background);
    }

    public override void OnPreviewSettings() //小界面横条设置内容
    {
        if (editor != null)
            editor.OnPreviewSettings();
    }

    public override string GetInfoString() 
    {
        return editor != null ? editor.GetInfoString() : base.GetInfoString();
    }

	}


代码很详细，要说的都在代码里，然后OK了。
但是，lua脚本过长，Unity里会报错没法绘制了，所以脚本大小要控制，进一步方法待思考开发。



----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

简单说一下Inspector面板下面的预览小面板，一般模型什么的会用到这个。

如，上述改为：

 	//是否有Inspector面板下面的预览小界面
    public override bool HasPreviewGUI()
    {
        return true;
       //return editor != null ? editor.HasPreviewGUI() : base.HasPreviewGUI();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        //if (editor != null)
        //    editor.OnPreviewGUI(r, background);
        GUI.Label(r, "Preview \n This is preview content!");
    }

    public override void OnPreviewSettings() //小界面横条设置内容
    {
        //if (editor != null)
        //    editor.OnPreviewSettings();
        GUIStyle preLabel = new GUIStyle("preLabel");
        GUIStyle preButton = new GUIStyle("preButton");

        GUILayout.Label("EanLabel", preLabel);
        GUILayout.Button("EamBtn", preButton);
    }

    public override string GetInfoString()
    {
        return "Here Info";// editor != null ? editor.GetInfoString() : base.GetInfoString();
    }

    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent("Here Title");
    }



结合图片说明一下：

![](https://i.imgur.com/gBI6lST.png)