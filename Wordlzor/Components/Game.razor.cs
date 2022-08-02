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
using BlazorApplicationInsights;

namespace Wordlzor.Components
{
    public struct Iteration
    {
        public Iteration(string word)
        {
            PrepareMaxAmountLetters(word);
        }
        Dictionary<char, int> MaxAmountLetters = new Dictionary<char, int>();
        Dictionary<char, int> CurrentAmountLetters = new Dictionary<char, int>();
        public void PrepareMaxAmountLetters(string currentWord)
        {
            foreach (var item in currentWord)
            {
                if (MaxAmountLetters.ContainsKey(item))
                {
                    MaxAmountLetters[item]++;
                }
                else
                {
                    MaxAmountLetters.Add(item, 1);
                }
            }
        }
        public void ResetCurrentAmountLetters()
        {
            CurrentAmountLetters = new Dictionary<char, int>();
        }
        public bool CheckLetter(char letter)
        {
            if (MaxAmountLetters.ContainsKey(letter))
            {
                if (!CurrentAmountLetters.ContainsKey(letter))
                {
                    CurrentAmountLetters.Add(letter, 1);
                    return true;
                }
                else
                {
                    if (CurrentAmountLetters[letter] < MaxAmountLetters[letter])
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

    }
    public partial class Game
    {
        [Parameter]
        public string LocalWordle { get; set; } = "";
        public string LocalTitle { get; set; }
        #region Misc

        /// <summary>
        /// Reference to main container
        /// </summary>
        private ElementReference _mainContainer;

        /// <summary>
        /// List of valid keys to press
        /// </summary>
        private List<string> _validKeys = new List<string>() { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "c", "v", "b", "n", "m" };
        #endregion

        #region Game state properties

        /// <summary>
        /// Word to guess
        /// </summary>
        private string _word = "dino";

        /// <summary>
        /// Game state
        /// </summary>
        private bool _finished;

        /// <summary>
        /// Iterations to guess the word
        /// </summary>
        private int _iterations = 6;


        /// <summary>
        /// Current game iteration
        /// </summary>
        private int _currentIteration = 0;

        private Iteration hasLetter;

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
        public HttpClient? HttpClient { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IApplicationInsights AppInsights { get; set; }

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            Random rnd = new Random();
            string FileUrl="";
            switch (LocalWordle)
            {
                case "isaac":
                    LocalTitle = "TBOI";
                    FileUrl = "isaac_words.json";
                    break;
                case "dota":
                    LocalTitle = "DOTA 2";
                    FileUrl = "dota_words.json";
                    break;
                default:
                    LocalTitle = "Dino's Cafe";
                    FileUrl = "words.json";
                    break;
            }
            // Load word list
            _wordList = await HttpClient.GetFromJsonAsync<List<string>>($"data/{FileUrl}");
            int maxnumber = _wordList.Count() + 1;
            _word = _wordList[(int)rnd.NextInt64(0, maxnumber)];
            hasLetter = new Iteration(_word);
            // Track event that game starter
            await AppInsights.TrackEvent("Game started");
        }

        #endregion

        #region Events

        /// <summary>
        /// Function to handle on key presses
        /// </summary>
        /// <param name="key">Key pressed</param>
        public async Task OnKeyboardClick(string key)
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
                        if (wordFromDictionaryLength == _word.Length)
                        {
                            // Check if the word is a valid 5 letter word
                            if (_wordList.Contains(wordFromDictionary))
                            {
                                // Check if we actually guessed the word
                                if (_wordDictionary[_currentIteration] == _word)
                                {
                                    // Update game state to not continue
                                    _finished = true;

                                    // Show message
                                    await JSRuntime.InvokeVoidAsync("alert", "Congratulations! You guessed the word!");

                                    // Track event that game finished
                                    await AppInsights.TrackEvent("Game finished");
                                }

                                // Check if we are on the last iteration
                                if (_currentIteration + 1 == _iterations)
                                {
                                    if (!_finished)
                                    {
                                        // Update game state to not continue
                                        _finished = true;

                                        // Show message
                                        await JSRuntime.InvokeVoidAsync("alert", "Unlucky! Try again!");

                                        // Track event that game ended
                                        await AppInsights.TrackEvent("Game ended");
                                    }
                                }

                                // In any case, if the word is correct and we didn't guess, next iteration
                                _currentIteration++;
                            }
                            else
                            {
                                // Not a valid word
                                await JSRuntime.InvokeVoidAsync("alert", "Not a valid word!");

                                // Track event that user entered a bad word
                                await AppInsights.TrackEvent("Bad word entered");
                            }
                        }
                        else
                        {
                            // Fill all tiles
                            await JSRuntime.InvokeVoidAsync("alert", $"Fill all the letters, it's a {_word.Length} letter word!");

                            // Track event that user didn't fill the entire word
                            await AppInsights.TrackEvent("Word not filled");
                        }
                    }
                    // This case is when the uses clicks on a letter from the keyboard
                    else
                    {
                        // If the word is less thant 5 letters
                        if (wordFromDictionaryLength <= _word.Length)
                        {
                            // Concat the new letter
                            _wordDictionary[_currentIteration] += key;

                            // Track event that user entered a letter
                            await AppInsights.TrackEvent("Letter entered: " + key);
                        }
                    }
                }
                // Create a new iteration
                else
                {
                    if (key == "⬅️" || key == "👌")
                    {
                        // Skip in this case
                    }
                    else
                    {
                        // Create new word
                        var newWord = key;

                        // Add to the dictionary of words with the new iteration
                        _wordDictionary[_currentIteration] = newWord;

                        // Track event that user started new iteration
                        await AppInsights.TrackEvent("New iteration started");
                    }
                }

                // Update UI, this will trigger GetLetterFromWord and GetTypeFromWord
                StateHasChanged();
            }
        }

        protected async Task KeyDown(KeyboardEventArgs e)
        {
            // If not finished
            if (!_finished)
            {
                // Edge case for enter
                if (e.Key.ToLowerInvariant() == "enter")
                {
                    await OnKeyboardClick("👌");
                }
                // Edge case for delete
                else if (e.Key.ToLowerInvariant() == "backspace")
                {
                    await OnKeyboardClick("⬅️");
                }
                // Send normal key
                else
                {
                    // Only if it's a valid key
                    if (_validKeys.Contains(e.Key))
                    {
                        await OnKeyboardClick(e.Key);
                    }
                }
            }
        }

        #endregion

        #region Utilities

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
            if (index==0)
            {
                hasLetter.ResetCurrentAmountLetters();
            }
            // Get index letter from user's word
            var indexInputLetterWord = word1[index].ToString().ToLowerInvariant();

            // Get index letter from correct word
            var indexWordLetter = word2[index].ToString().ToLowerInvariant();

            // If they match, correct word
            if (indexInputLetterWord == indexWordLetter)
            {
                // Set matched list for correct
                SetMatchedLists(letter, "Correct");
                if (hasLetter.CheckLetter(Convert.ToChar(letter)))
                {
                    // Return correct for styling purposes
                    return "Correct";
                }
                else
                {
                    return "Absent";
                }
            }
            else if (word2.ToLowerInvariant().Contains(letter))
            {
                // Set matched list for present
                SetMatchedLists(letter, "Present");

                if (hasLetter.CheckLetter(Convert.ToChar(letter)))
                {
                    // Return correct for styling purposes
                    return "Present";
                }
                else
                {
                    return "Absent";
                }
                
            }
            else
            {
                // Set matched list for absent
                SetMatchedLists(letter, "Absent");

                if (hasLetter.CheckLetter(Convert.ToChar(letter)))
                {
                    // Return correct for styling purposes
                    return "Absent";
                }
                else
                {
                    return "Absent";
                }
                
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

        public async Task Focus()
        {
            // Focus when initializing
            await JSRuntime.InvokeVoidAsync("window.FocusElement", _mainContainer);
        }

        #endregion
    }
}