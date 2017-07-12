using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceFramework
{
    public class ArgumentsService
    {
        public ShellType Shell { get; set; } = ShellType.PowerShell;

        public String[] Generate(String str)
        {
            var input = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var arglst = new List<String>();
            for (int i = 0; i < input.Count(); i++)
            {
                if (Shell == ShellType.Terminal)
                {
                    var arg = input[i];
                    while (input[i].Last() == '\\')
                    {
                        arg += $" {input[i + 1]}";
                        i++;
                    }
                    arglst.Add(arg);
                }
                else if(Shell == ShellType.PowerShell)
                {
                    if (input[i].First() != '\"')
                    {
                        arglst.Add(input[i]);
                    }
                    else if (input[i].Last() == '\"')
                    {
                        arglst.Add(input[i].Replace("\"", ""));
                    }
                    else
                    {
                        var next = input.FindIndex(i + 1, x => x.Last() == '\"');
                        arglst.Add(String.Join(" ", input.Skip(i).Take(next - i + 1)).Replace("\"", ""));
                        i = next;
                    }
                }
            }
            return arglst.ToArray();
        }
    }

    public enum ShellType
    {
        PowerShell = 0,
        Terminal = 1
    }
}
