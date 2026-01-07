/*
 *1: Run the code 
 */
// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Hello, World!");

#region
string packageId = "Microsoft.FoundryLocal";
Process process = new()
{
    StartInfo = new ProcessStartInfo
    {
        FileName = ""
    }
};
#endregion