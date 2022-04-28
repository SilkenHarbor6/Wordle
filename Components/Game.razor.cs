using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Wordlzor;

namespace Wordlzor.Components
{
    public partial class Game
    {
        private string word = "house";
        private bool finished;
        private int Iterations = 5;
        private int CurrentIteration = 0;
        private Dictionary<int, string> wordDictionary = new Dictionary<int, string>();

        public List<string> MatchedWords { get; set; } = new();
        public List<string> PresentWords { get; set; } = new();
        public List<string> AbsentWords { get; set; } = new();

        public void OnKeyboardClick(string key)
        {
            if (!finished)
            {
                if (wordDictionary.ContainsKey(CurrentIteration))
                {
                    var wordFromDictionary = wordDictionary[CurrentIteration];
                    var wordFromDictionaryLength = wordFromDictionary.Length;

                    if (key == "⬅️")
                    {
                        if (wordFromDictionaryLength > 0)
                        {
                            wordDictionary[CurrentIteration] = wordFromDictionary.Remove(wordFromDictionary.Length - 1, 1);
                        }
                    }
                    else if (key == "👌")
                    {
                        if (wordFromDictionaryLength == 5)
                        {
                            if (wordDictionary[CurrentIteration] == word)
                            {
                                finished = true;
                            }

                            CurrentIteration++;
                        }
                    }
                    else
                    {
                        if (wordFromDictionaryLength <= 5)
                        {
                            wordDictionary[CurrentIteration] += key;
                        }
                    }
                }
                else
                {
                    var newWord = key;
                    wordDictionary[CurrentIteration] = newWord;
                }
            }

            StateHasChanged();
        }

        public string GetLetterFromWord(int index, int iteration)
        {
            if (CurrentIteration == iteration)
            {
                if (wordDictionary.ContainsKey(CurrentIteration))
                {
                    var wordFromDictionary = wordDictionary[CurrentIteration];

                    return wordFromDictionary.Length > index ? wordFromDictionary[index].ToString() : "";
                }
            }
            else
            {
                if (wordDictionary.ContainsKey(iteration))
                {
                    return wordDictionary[iteration][index].ToString();
                }
            }

            return "";
        }

        public string GetTypeFromWord(int index, string letter, int iteration)
        {
            if (CurrentIteration == iteration)
            {
                if (wordDictionary.ContainsKey(CurrentIteration))
                {
                    var wordFromDictionary = wordDictionary[CurrentIteration];

                    if (wordFromDictionary.Length > index)
                    {
                        return "Idle";
                    }
                }
            }
            else
            {
                if (wordDictionary.ContainsKey(iteration))
                {
                    return GetType(wordDictionary[iteration], word, letter, index);
                }
            }

            return "";
        }

        private string GetType(string word1, string word2, string letter, int index)
        {
            var indexInputLetterWord = word1[index].ToString().ToLowerInvariant();
            var indexWordLetter = word2[index].ToString().ToLowerInvariant();

            if (indexInputLetterWord == indexWordLetter)
            {
                SetMatchedLists(letter, "Correct");
                return "Correct";
            }
            else if (word2.ToLowerInvariant().Contains(letter))
            {
                SetMatchedLists(letter, "Present");
                return "Present";
            }
            else
            {
                SetMatchedLists(letter, "Absent");
                return "Absent";
            }
        }

        private void SetMatchedLists(string key, string type)
        {
            // Remove from all list, then re-add
            MatchedWords.RemoveAll(x => x == key);
            PresentWords.RemoveAll(x => x == key);
            AbsentWords.RemoveAll(x => x == key);

            switch (type)
            {
                case "Correct":
                    MatchedWords.Add(key);
                    break;
                case "Present":
                    PresentWords.Add(key);
                    break;
                case "Absent":
                    AbsentWords.Add(key);
                    break;
                default:
                    break;
            }
        }
    }
}