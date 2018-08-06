using System;
using System.Collections.Generic;

namespace RedisCacheConsole
{
    [DecayTime(Seconds = 5)]
    public class Student
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateofBirth { get; set; }
        public Faculty Faculty { get; set; }
        public Subject[] Subjects { get; set; }
        public Course Course { get; set; }
    }

    public enum Course
    {
        First, Second, Third, Fourth
    }

    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfFoundation { get; set; }
    }

    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}