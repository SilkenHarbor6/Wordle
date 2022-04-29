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
        #region Game state properties

        /// <summary>
        /// Word to guess
        /// </summary>
        private string _word = "house";

        /// <summary>
        /// Game state
        /// </summary>
        private bool _finished;

        /// <summary>
        /// Iterations to guess the word
        /// </summary>
        private int _iterations = 5;


        /// <summary>
        /// Current game iteration
        /// </summary>
        private int _currentIteration = 0;


        /// <summary>
        /// Dictionary of guesses, for each entry we have a word
        /// </summary>
        private Dictionary<int, string> _wordDictionary = new Dictionary<int, string>();

        private List<string> _wordList = new();

        #endregion

        #region Properties for other components

        /// <summary>
        /// List of correct letters
        /// </summary>
        public List<string> MatchedLetters { get; set; } = new();

        /// <summary>
        /// List of present letters
        /// </summary>
        public List<string> PresentLetters { get; set; } = new();

        /// <summary>
        /// List of absent letters
        /// </summary>
        public List<string> AbsentLetters { get; set; } = new();

        #endregion

        #region Injects

        [Inject]
        public HttpClient HttpClient { get; set; }

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            _wordList = await HttpClient.GetFromJsonAsync<List<string>>("data/words.json");
        }

        #endregion

        public void OnKeyboardClick(string key)
        {
            // Check if the game is not finished
            if (!_finished)
            {
                // Check if the iteration is in the dictionary
                if (_wordDictionary.TryGetValue(_currentIteration, out string wordFromDictionary))
                {
                    // Get the length of it
                    var wordFromDictionaryLength = wordFromDictionary.Length;

                    // Edge case when user clicks on DELETE
                    if (key == "⬅️")
                    {
                        // Only if the word actually has content
                        if (wordFromDictionaryLength > 0)
                        {
                            // Remove a letter starting from the end
                            _wordDictionary[_currentIteration] = wordFromDictionary.Remove(wordFromDictionary.Length - 1, 1);
                        }
                    }
                    // Edge case if the user clicks on START
                    else if (key == "👌")
                    {
                        // Only if the word is completed
                        if (wordFromDictionaryLength == 5)
                        {
                            // Check if the word is a valid 5 letter word
                            if (_wordList.Contains(wordFromDictionary))
                            {
                                // Check if we actually guessed the word
                                if (_wordDictionary[_currentIteration] == _word)
                                {
                                    // Update game state to not continue
                                    _finished = true;
                                }

                                // In any case, if the word is correct and we didn't guess, next iteration
                                _currentIteration++;
                            }
                        }
                    }
                    // This case is when the uses clicks on a letter from the keyboard
                    else
                    {
                        // If the word is less thant 5 letters
                        if (wordFromDictionaryLength <= 5)
                        {
                            // Concat the new letter
                            _wordDictionary[_currentIteration] += key;
                        }
                    }
                }
                // Create a new iteration
                else
                {
                    // Create new word
                    var newWord = key;

                    // Add to the dictionary of words with the new iteration
                    _wordDictionary[_currentIteration] = newWord;
                }

                // Update UI, this will trigger GetLetterFromWord and GetTypeFromWord
                StateHasChanged();
            }
        }

        /// <summary>
        /// Function that returns letter from the current iteration's word
        /// </summary>
        /// <param name="index">Char position in the word</param>
        /// <param name="iteration">Game iteration</param>
        /// <returns>Letter from word</returns>
        public string GetLetterFromWord(int index, int iteration)
        {
            // Check if the iteration is in the dictionary
            if (_wordDictionary.TryGetValue(iteration, out string wordFromDictionary))
            {
                // This is for the current iteration
                if (_currentIteration == iteration)
                {
                    // Return letter for the index we are in the loop, we have to check for the length
                    return wordFromDictionary.Length > index ? wordFromDictionary[index].ToString() : "";

                }
                // This is for the older iterations
                else
                {
                    // If it exists, the word is already 5 letters and we have to return the letters from it
                    return _wordDictionary[iteration][index].ToString();
                }
            }

            // In any other case, return nothing
            return string.Empty;
        }

        /// <summary>
        /// Get type from word introduced
        /// </summary>
        /// <param name="index">Index of char in word</param>
        /// <param name="letter">Char of word</param>
        /// <param name="iteration">Iteration</param>
        /// <returns>Style from word</returns>
        public string GetTypeFromWord(int index, string letter, int iteration)
        {
            // Check if the iteration is in the dictionary
            if (_wordDictionary.TryGetValue(iteration, out string wordFromDictionary))
            {
                // This is for the current iteration
                if (_currentIteration == iteration)
                {
                    // Check the lenght and compare it to the index
                    if (wordFromDictionary.Length > index)
                    {
                        // Return class that will be used
                        return "Idle";
                    }
                }
                // This is for the older iterations
                else
                {
                    // Return type for older words
                    return GetType(_wordDictionary[iteration], _word, letter, index);
                }
            }

            // In any other case, return nothing
            return string.Empty;
        }

        /// <summary>
        /// Function that returns the type, this decides the style on the tiles and the keyboard
        /// </summary>
        /// <param name="word1">Word from the user's play</param>
        /// <param name="word2">Correct word</param>
        /// <param name="letter">Letter that the user played</param>
        /// <param name="index">Current index in the word</param>
        /// <returns></returns>
        private string GetType(string word1, string word2, string letter, int index)
        {
            // Get index letter from user's word
            var indexInputLetterWord = word1[index].ToString().ToLowerInvariant();

            // Get index letter from correct word
            var indexWordLetter = word2[index].ToString().ToLowerInvariant();

            // If they match, correct word
            if (indexInputLetterWord == indexWordLetter)
            {
                // Set matched list for correct
                SetMatchedLists(letter, "Correct");

                // Return correct for styling purposes
                return "Correct";
            }
            else if (word2.ToLowerInvariant().Contains(letter))
            {
                // Set matched list for present
                SetMatchedLists(letter, "Present");

                // Return present for styling purposes
                return "Present";
            }
            else
            {
                // Set matched list for absent
                SetMatchedLists(letter, "Absent");

                // Return absent for styling purposes
                return "Absent";
            }
        }

        /// <summary>
        /// Updates the lists for the keyboard
        /// </summary>
        /// <param name="key">Letter</param>
        /// <param name="type">Type</param>
        private void SetMatchedLists(string key, string type)
        {
            // Remove from all list,, because it could have been in one place already
            MatchedLetters.RemoveAll(x => x == key);
            PresentLetters.RemoveAll(x => x == key);
            AbsentLetters.RemoveAll(x => x == key);

            // Depending on the type, add to specific list
            switch (type)
            {
                case "Correct":
                    MatchedLetters.Add(key);
                    break;
                case "Present":
                    PresentLetters.Add(key);
                    break;
                case "Absent":
                    AbsentLetters.Add(key);
                    break;
                default:
                    break;
            }
        }
    }
}