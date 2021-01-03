using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlAVP.Pages
{
    public class TailCommandProcessorModel : PageModel
    {
        public Guid Id { get; private set; }
        public void OnGet(Guid id)
        {
            Id = id;
        }
    }
}
