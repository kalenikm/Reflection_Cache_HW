using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionComparer
{
    public class Comparer<T> : EqualityComparer<T>
    {
        public override bool Equals(T x, T y)
        {
            return Compare(typeof(T), x, y);
        }

        public override int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

        private bool Compare(Type type, object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) != ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            type = type != x.GetType() ? x.GetType() : type;

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return CompareEnumerables(type, x, y);
            }

            if (type.IsPrimitive || type.IsEnum)
            {
                return x.Equals(y);
            }

            if (typeof(DateTime).IsAssignableFrom(type))
            {
                return x.Equals(y);
            }

            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                return CompareByProperties(type, x, y);
            }

            return x.Equals(y);
        }

        private bool CompareEnumerables(Type type, object x, object y)
        {
            var genEnumerable = type.GetInterfaces().SingleOrDefault(i =>
                i.IsConstructedGenericType && i.GetInterfaces().First() == typeof(IEnumerable));

            var genType = genEnumerable.GetGenericArguments()[0];

            int countOfArgs;
            object[] arguments;

            if (genType.IsPrimitive)
            {
                countOfArgs = 2;
                arguments = new[] { x, y };
            }
            else
            {
                countOfArgs = 3;
                arguments = new[] { x, y, this };
            }

            var genericSequenceEqual = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "SequenceEqual" && m.GetParameters().Length == countOfArgs);

            var sequenceEqualMethod = genericSequenceEqual.MakeGenericMethod(genType);

            return (bool)sequenceEqualMethod.Invoke(null, arguments);
        }

        private bool CompareByProperties(Type type, object x, object y)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.GetIndexParameters().Length == 0).ToList();

            foreach (var propInfo in properties)
            {
                if (!this.Compare(propInfo.PropertyType, propInfo.GetValue(x), propInfo.GetValue(y)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
