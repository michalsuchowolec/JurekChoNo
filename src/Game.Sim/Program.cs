using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Core;
using System.Text;
using MatchEngine;


var board = new Board(HexArea.Circle(7));
var hexPoseModifierFactory = new HexPoseModifierFactory(new TimeStampTracker());  
var movementResolver = new MovementResolver(board, hexPoseModifierFactory);

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