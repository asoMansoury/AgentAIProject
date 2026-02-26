using System;
using System.Collections.Generic;
using System.Text;

namespace ToolCalling.FromAnMcpServer.Models
{
    public class Movie
    {
        public required string Title { get; set; }
        public int YearOfRelease { get; set; }
        public required string Director { get; set; }
        public MovieGenre Genre { get; set; }

        public decimal ImdbScore { get; set; }
    }
}
