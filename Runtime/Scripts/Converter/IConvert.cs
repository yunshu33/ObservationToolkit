using UnityEngine;

namespace LJVoyage.ObservationToolkit.Runtime.Converter
{
    public interface IConvert<Source, Target>
    {
        Source ObjectConvertSource(object source);

        Target SourceConvertTarget(Source source);
        
        Source TargetConvertSource(Target target);
        
        
    }

    public class Convert1 : IConvert<int, string>
    {
        public int ObjectConvertSource(object source)
        {
            return (int)(source);
        }

        public string SourceConvertTarget(int source)
        {
            return source.ToString();
        }

        public int TargetConvertSource(string target)
        {
            return int.Parse(target);
        }
    }
}