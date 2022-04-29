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
    public partial class Keyboard
    {
        #region Keyboard rows

        /// <summary>
        /// The rows of the keyboard
        /// </summary>
        private readonly List<string> FirstRow = new List<string>() { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", };

        /// <summary>
        /// The rows of the keyboard
        /// </summary>
        private readonly List<string> SecondRow = new List<string>() { "a", "s", "d", "f", "g", "h", "j", "k", "l", };

        /// <summary>
        /// The rows of the keyboard
        /// </summary>
        private readonly List<string> ThirdRow = new List<string>() { "👌", "z", "x", "c", "v", "b", "n", "m", "⬅️" };

        #endregion

        #region Parameters

        /// <summary>
        /// The current word being typed
        /// </summary>
        [Parameter]
        public EventCallback<string> OnKeyPressed { get; set; }

        /// <summary>
        /// Absent keys from game
        /// </summary>
        [Parameter]
        public List<string> AbsentKeys { get; set; } = new();

        /// <summary>
        /// Present keys from game
        /// </summary>
        [Parameter]
        public List<string> PresentKeys { get; set; } = new();

        /// <summary>
        /// Correct keys from game
        /// </summary>
        [Parameter]
        public List<string> CorrectKeys { get; set; } = new();

        #endregion

        #region Utilities

        /// <summary>
        /// Gets the keyboard css depending ont the keys introduced
        /// </summary>
        /// <param name="key">Key from keyboard</param>
        /// <returns>Css class</returns>
        public string GetCss(string key)
        {
            if (AbsentKeys.Contains(key))
            {
                return "button-absent";
            }
            else if (PresentKeys.Contains(key))
            {
                return "button-present";
            }
            else if (CorrectKeys.Contains(key))
            {
                return "button-correct";
            }

            return string.Empty;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event to callback game's event on key pressed
        /// </summary>
        /// <param name="key"></param>
        public void KeyClicked(string key) => OnKeyPressed.InvokeAsync(key);

        #endregion
    }
}