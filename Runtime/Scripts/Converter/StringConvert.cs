namespace LJVoyage.ObservationToolkit.Runtime.Converter
{
    public class StringConvert : IConvert<string, int>, IConvert<string, float>, IConvert<string, double>
    {
        string IConvert<string, int>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        string IConvert<string, float>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        string IConvert<string, double>.ObjectConvertSource(object source)
        {
            throw new System.NotImplementedException();
        }

        int IConvert<string, int>.SourceConvertTarget(string source)
        {
            return int.Parse(source);
        }

        public string TargetConvertSource(double target)
        {
            throw new System.NotImplementedException();
        }

        public string TargetConvertSource(float target)
        {
            throw new System.NotImplementedException();
        }

        public string TargetConvertSource(int target)
        {
            throw new System.NotImplementedException();
        }

        float IConvert<string, float>.SourceConvertTarget(string source)
        {
            return float.Parse(source);
        }

        double IConvert<string, double>.SourceConvertTarget(string source)
        {
            return double.Parse(source);
        }
    }
}