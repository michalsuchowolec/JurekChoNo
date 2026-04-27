using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Core;
using System.Text;

var solution = new Solution();

Console.WriteLine(solution.GenerateString("TFTF", "ab"));

public class Solution {
    public string GenerateString(string str1, string str2) {
        var deque = new LinkedList<int>();
        var result = new StringBuilder();
        for(int i = 0; i < str1.Length + str2.Length - 1; i++){
            if(i < str1.Length && str1[i] == 'T'){
                Console.WriteLine("been here");
                deque.AddLast(0);
            }
            if(deque.Count == 0){
                result.Append('a');
            }
            else{
                var letter = str2[deque.First()];
                foreach(var index in deque){
                    if(str2[index] != letter) return ""; 
                }
                result.Append(letter);
                deque = new LinkedList<int>(deque.Select(x => x + 1));
                if(deque.First() == str2.Length) deque.RemoveFirst();
            }
            // Console.WriteLine($"{i}: \n(");
            // foreach(var element in deque){
            //     Console.Write(element);
            // }
            // Console.WriteLine(")");
        }
        return result.ToString();
    }
}

