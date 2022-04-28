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
        public List<string> FirstRow = new List<string>() { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", };
        public List<string> SecondRow = new List<string>() { "a", "s", "d", "f", "g", "h", "j", "k", "l", };
        public List<string> ThirdRow = new List<string>() { "👌", "z", "x", "c", "v", "b", "n", "m", "⬅️" };

    }
}