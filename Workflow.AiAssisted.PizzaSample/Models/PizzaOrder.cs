using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow.AiAssisted.PizzaSample.Models
{
    public class PizzaOrder
    {
        public PizzaSize Size { get; set; }
        public List<string> Toppings { get;set; }
        public Dictionary<WarningType, string> Warnings { get; set; }

    }
}
