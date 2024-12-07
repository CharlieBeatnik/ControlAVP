using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlAVP.Pages
{
    internal sealed class TailCommandProcessorModel : PageModel
    {
        public Guid Id { get; private set; }
        public string DisplayName { get; private set; }
        public string ImagePath { get; private set; }

        public void OnGet(Guid id, string displayName, string imagePath)
        {
            Id = id;
            DisplayName = displayName;
            ImagePath = imagePath;
        }
    }
}
