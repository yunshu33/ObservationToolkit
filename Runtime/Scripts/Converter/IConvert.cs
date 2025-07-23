namespace LJVoyage.ObservationToolkit.Runtime.Converter
{
    public interface IConvert<Source, out Target>
    {
        Source Convert(object source);

        Target Convert(Source source);
    }

    public class Convert1 : IConvert<int, string>
    {
        public int Convert(object source)
        {
            return (int)(source);
        }

        public string Convert(int source)
        {
            return source.ToString();
        }
    }
}