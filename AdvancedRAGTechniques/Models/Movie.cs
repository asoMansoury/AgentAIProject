using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UsingRagInAgentFramework
{
    [DebuggerDisplay("{Title} - {Plot}")]
    public class Movie
    {
        public required string Title { get; set; }
        public required string Plot { get; set; }
        public required decimal Rating { get; set; }

        public string GetTitleAndDetails()
        {
            return $"Title: {Title} - Rating: {Rating} - Plot: {Plot}";
        }
    }
}
