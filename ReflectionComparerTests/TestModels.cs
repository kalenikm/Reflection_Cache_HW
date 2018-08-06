using System;

namespace ReflectionComparerTests
{
    public class Model
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public int[] Prop3 { get; set; }
        public Model2 Prop4 { get; set; }
    }

    public class Model2
    {
        public DateTime Prop1 { get; set; }
        public string Prop2 { get; set; }
        public int Prop3 { get; set; }
        public Model3 Prop4 { get; set; }
    }

    public class Model3
    {
        public char Prop1 { get; set; }
        public string Prop2 { get; set; }
        public int Prop3 { get; set; }
        public double? Prop4 { get; set; }
    }
}