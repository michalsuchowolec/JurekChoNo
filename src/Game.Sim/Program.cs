using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Core;
using System.Text;
using MatchEngine;


var matchRoot = new MatchRoot(new List<InitialPlayerData> { new(new List<string>()), new(new List<string>()) });

Console.WriteLine("hello world");

var person = new Person(3, b : 1);

public class Person
{
    public int Age;
    public int DateOfBirth;

    public Person(int a, int b = 3)
    {
        
    }
}