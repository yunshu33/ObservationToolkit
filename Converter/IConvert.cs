namespace MvvmToolkit.Converter
{
    public interface IConvert<in Source, out Target>
    {
        Target Convert(Source source);
    }
    
    public class Convert1 : IConvert<int, string>
    {
        public string Convert(int source)
        {
            return source.ToString();
        }
    }
}