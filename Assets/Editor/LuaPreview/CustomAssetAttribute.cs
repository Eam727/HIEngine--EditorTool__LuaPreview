/// <summary>
/// 资源Inspector预览属性
/// </summary>
public class CustomAssetAttribute : System.Attribute
{

    public string[] extensions; //扩展名

    public CustomAssetAttribute(params string[] extension)
    {
        this.extensions = extension;
    }
}
