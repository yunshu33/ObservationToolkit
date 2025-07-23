using LJVoyage.ObservationToolkit.Runtime.Converter;

namespace LJVoyage.ObservationToolkit.Runtime
{
    public interface IOneWayBinder<S, SProperty, in TProperty>
    {
        void OneWay();


        void OneWay(IConvert<SProperty, TProperty> convert);
    }
}