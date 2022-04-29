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
    public partial class Modal
    {
        [Parameter]
        public EventCallback OnClose { get; set; }

        public bool Display { get; set; } = true;

        public void Close()
        {
            Display = false;
            OnClose.InvokeAsync(null);
        }

        public string GetCss() => Display ? "d-inline" : "d-none";
    }
}