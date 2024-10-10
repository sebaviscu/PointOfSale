using System;

namespace PointOfSale.Model.Configuracion
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class NonUpdatableAttribute : Attribute
    {
        public NonUpdatableAttribute() { }
    }
}

