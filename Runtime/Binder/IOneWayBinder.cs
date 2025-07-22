using MvvmToolkit.Converter;

namespace MvvmToolkit
{
    public interface IOneWayBinder<S, SProperty, in TProperty> 
    {
        void OneWay();
        
        
        void OneWay(IConvert<SProperty, TProperty> convert);
        
    }
}