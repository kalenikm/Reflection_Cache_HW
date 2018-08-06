using System;
using NUnit.Framework;
using ReflectionComparer;

namespace ReflectionComparerTests
{
    [TestFixture]
    public class ComparerTests
    {
        private Model _obj1;
        private Model _obj2;

        [SetUp]
        public void Init()
        {
            _obj1 = new Model()
            {
                Prop1 = "test",
                Prop2 = 1,
                Prop3 = new int[] { 1, 2, 3 },
                Prop4 = new Model2()
                {
                    Prop1 = new DateTime(1, 1, 1),
                    Prop2 = null,
                    Prop3 = 5,
                    Prop4 = new Model3()
                    {
                        Prop1 = 'a',
                        Prop2 = "123",
                        Prop3 = 99,
                        Prop4 = 23.2D
                    }
                }
            };

            _obj2 = new Model()
            {
                Prop1 = "test",
                Prop2 = 1,
                Prop3 = new int[] { 1, 2, 3 },
                Prop4 = new Model2()
                {
                    Prop1 = new DateTime(1, 1, 1),
                    Prop2 = null,
                    Prop3 = 5,
                    Prop4 = new Model3()
                    {
                        Prop1 = 'a',
                        Prop2 = "123",
                        Prop3 = 99,
                        Prop4 = 23.2D
                    }
                }
            };
        }

        [Test]
        public void CompareMethod_SameReferences_ReturnsTrue()
        {
            Assert.IsTrue(_obj1.DeepCompare(_obj1));
        }

        [Test]
        public void CompareMethod_DifferentTypes_ReturnsFalse()
        {
            var x = (object) _obj1;
            var y = (object) 5;

            Assert.IsFalse(x.DeepCompare(y));
        }

        [Test]
        public void CompareMethod_NullAsParameter_ReturnsFalse()
        {
            _obj2 = null;
            Assert.IsFalse(_obj1.DeepCompare(_obj2));
        }

        [Test]
        public void CompareMethod_ObjectsWithDifferentProps_ReturnsFalse()
        {
            _obj2.Prop2 = _obj2.Prop2 + 1;
            _obj2.Prop4.Prop4.Prop2 = "TEST";
            Assert.IsFalse(_obj1.DeepCompare(_obj2));
        }

        [Test]
        public void CompareMethod_ObjectsWithSameProps_ReturnsTrue()
        {
            Assert.IsTrue(_obj1.DeepCompare(_obj2));
        }
    }
}
