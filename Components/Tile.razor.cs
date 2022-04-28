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
    public partial class Tile
    {
        [Parameter]
        public string Letter { get; set; }

        [Parameter]
        public string Type { get; set; }

        // Get css based on type
        public string GetCss() => string.IsNullOrEmpty(Type) ? "tile-absent" : $"tile-{Type.ToLower()}";
    }
}