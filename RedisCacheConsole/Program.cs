using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ReflectionCache;
using StackExchange.Redis;

namespace RedisCacheConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            for (int i = 0; i < 5; i++)
            {
                stopwatch.Restart();
                var student = GetStudentById("STDNT1");
                stopwatch.Stop();
                Console.WriteLine($"Student {student.Id}: {student.FirstName} {student.LastName}; Time: {stopwatch.Elapsed}");
            }

            for (int i = 0; i < 5; i++)
            {
                stopwatch.Restart();
                var student = GetStudentById("STDNT2");
                stopwatch.Stop();
                Console.WriteLine($"Student {student.Id}: {student.FirstName} {student.LastName}; Time: {stopwatch.Elapsed}");
            }

            for (int i = 0; i < 5; i++)
            {
                stopwatch.Restart();
                var student = GetStudentById("STDNT1");
                stopwatch.Stop();
                Console.WriteLine($"Student {student.Id}: {student.FirstName} {student.LastName}; Time: {stopwatch.Elapsed}");
            }

            Console.ReadKey();
        }

        public static Student GetStudentById(string id)
        {
            using (var redis = ConnectionMultiplexer.Connect("localhost"))
            {
                var db = redis.GetDatabase();

                var result = db.StringGet(id);
                var xmlDoc = XDocument.Parse(result.ToString());
                var strTime = xmlDoc.Root?.Attribute("expTime")?.Value;
                var timeExist = DateTime.TryParse(strTime, out var time);

                if (result.IsNullOrEmpty || !timeExist || GetExpTime() <= DateTime.UtcNow - time.ToUniversalTime()) 
                {
                    var student = GetStudentWithWait(id);
                    var xmlStudent = student.ToXml();
                    xmlStudent.Root?.Add(new XAttribute("expTime", DateTime.UtcNow));
                    db.StringSet(id, xmlStudent.ToString());
                    return student;
                }

                return (Student)xmlDoc.ToModel<Student>();
            }
        }

        public static Student GetStudentWithWait(string id)
        {
            Task.Delay(3000).Wait();
            return Students.SingleOrDefault(st => st.Id == id);
        }

        public static TimeSpan GetExpTime()
        {
            var attribute = (DecayTime)typeof(Student).GetCustomAttributes(typeof(DecayTime), true).FirstOrDefault();
            return attribute?.Time() ?? TimeSpan.MinValue;
        }

        private static readonly Student[] Students = new Student[]
        {
            new Student()
            {
                Id = "STDNT1",
                FirstName = "Jhon",
                LastName = "Smith",
                DateofBirth = new DateTime(2001, 7, 5),
                Faculty = new Faculty()
                {
                    Id = 1,
                    Name = "EF",
                    DateOfFoundation = new DateTime(1920, 1, 1)
                },
                Subjects = new Subject[]
                {
                    new Subject()
                    {
                        Id = 1,
                        Name = "Math"
                    },
                    new Subject()
                    {
                        Id = 2,
                        Name = "Physics"
                    }
                },
                Course = Course.First
            },
            new Student()
            {
                Id = "STDNT2",
                FirstName = "Steve",
                LastName = "Smith",
                DateofBirth = new DateTime(2000, 12, 11),
                Faculty = new Faculty()
                {
                    Id = 1,
                    Name = "FKP",
                    DateOfFoundation = new DateTime(1920, 1, 1)
                },
                Subjects = new Subject[]
                {
                    new Subject()
                    {
                        Id = 1,
                        Name = "Math"
                    },
                    new Subject()
                    {
                        Id = 2,
                        Name = "Physics"
                    }
                },
                Course = Course.Second
            },
            new Student()
            {
                Id = "STDNT3",
                FirstName = "Steve",
                LastName = "White",
                DateofBirth = new DateTime(1999, 3, 25),
                Faculty = new Faculty()
                {
                    Id = 1,
                    Name = "FITR",
                    DateOfFoundation = new DateTime(1920, 1, 1)
                },
                Subjects = new Subject[]
                {
                    new Subject()
                    {
                        Id = 1,
                        Name = "Math"
                    },
                    new Subject()
                    {
                        Id = 2,
                        Name = "Physics"
                    }
                },
                Course = Course.Second
            }
        };
    }
}
