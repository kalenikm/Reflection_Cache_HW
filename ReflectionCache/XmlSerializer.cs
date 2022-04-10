using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ReflectionCache
{
    public static class XmlSerializer
    {
        public static XDocument ToXml<T>(this T obj)
        {
            if (ReferenceEquals(obj, null))
            {
                throw new ArgumentNullException();
            }

            return new XDocument(ToXml(typeof(T), obj, typeof(T).Name));
        }

        private static XElement ToXml(Type type, object obj, string name)
        {
            if (ReferenceEquals(obj, null))
            {
                return new XElement(name, new XAttribute("type", type.FullName), null);
            }

            if (type.IsPrimitive || type.IsEnum)
            {
                return new XElement(name, new XAttribute("type", type.FullName), obj);
            }

            if (type == typeof(string))
            {
                return new XElement(name, new XAttribute("type", type.FullName), obj);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ToXmlEnumerable(type, obj, name);
            }

            if (typeof(DateTime).IsAssignableFrom(type))
            {
                return new XElement(name, new XAttribute("type", type.FullName), ((DateTime) obj).ToString());
            }

            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                return ToXmlProps(type, obj, name);
            }

            throw new InvalidCastException();
        }

        private static XElement ToXmlEnumerable(Type type, object obj, string name)
        {
            var enumerable = (IEnumerable) obj;
            var elems = new List<XElement>();

            foreach (var item in enumerable)
            {
                elems.Add(ToXml(item.GetType(), item, item.GetType().Name));
            }

            return new XElement(name, new XAttribute("type", type.FullName), elems);
        }

        private static XElement ToXmlProps(Type type, object obj, string name)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var elems = new List<XElement>();

            foreach (var prop in props)
            {
                elems.Add(ToXml(prop.PropertyType, prop.GetValue(obj), prop.Name));
            }

            return new XElement(type.Name, new XAttribute("type", type.FullName), elems);
        }

        public static T ToModel<T>(this XDocument doc)
        {
            var root = doc.Root;

            return (T)ToModel(root, null, typeof(T));
        }

        private static object ToModel(XElement elem, object obj, Type type)
        {
            if (type.IsPrimitive)
            {
                return elem.Value;
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, elem.Value);
            }

            if (type == typeof(string))
            {
                return elem.Value;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ToModelArray(elem, obj, type);
            }

            if (typeof(DateTime).IsAssignableFrom(type))
            {
                return elem.Value;
            }

            if (type.IsClass || type.IsInterface || type.IsValueType)
            {
                obj = Activator.CreateInstance(type);
                return ToModelProps(elem, obj, type);
            }

            return null;
        }

        private static object ToModelProps(XElement elem, object obj, Type type)
        {
            var elems = elem.Elements().ToList();

            foreach (var el in elems)
            {
                var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .SingleOrDefault(p => p.PropertyType.FullName == el.Attribute("type")?.Value && p.Name == el.Name);

                if (ReferenceEquals(prop, null))
                {
                    continue;
                }

                var value = ToModel(el, obj, prop.PropertyType);

                if (!prop.PropertyType.GetInterfaces().Contains(typeof(IConvertible)))
                {
                    continue;
                }

                var converted = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(obj, converted);
            }

            return obj;
        }

        private static IList ToModelArray(XElement elem, object obj, Type type)
        {
            var elems = elem.Elements().ToList();
            var rootType = type;
            var ctorCount = rootType.GetConstructors().Length;
            var elemType = type.GetElementType() ?? type.GenericTypeArguments[0];

            var prop = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(p => p.PropertyType.FullName == elem.Attribute("type")?.Value && p.Name == elem.Name);

            if (ReferenceEquals(prop, null))
            {
                return null;
            }

            var list = ctorCount > 0 ? (IList)Activator.CreateInstance(rootType, elems.Count) : (IList)Activator.CreateInstance(elemType.MakeArrayType(), elems.Count);

            if (list.IsFixedSize)
            {
                for (int i = 0; i < elems.Count; i++)
                {
                    list[i] = ToModel(elems[i], obj, elemType);
                }
            }
            else
            {
                foreach (var el in elems)
                {
                    list.Add(ToModel(el, obj, elemType));
                }
            }

            prop.SetValue(obj, list);

            return list;
        }
    }
}
