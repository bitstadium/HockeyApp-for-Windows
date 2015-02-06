using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Args.Help;
using Args.Help.Formatters;

namespace HockeyApp.AppLoader.Util
{
    public class ConsoleHelpFormatterCustom : ConsoleHelpFormatter
    {

        private string _cmdName = "";
        public ConsoleHelpFormatterCustom()
            : base(Console.BufferWidth, 1, 5)
        {
            
        }

        public ConsoleHelpFormatterCustom(int bufferWidth, int commandSamplePadding, int argumentDescriptionPadding, string commandName)
            : base(bufferWidth, commandSamplePadding, argumentDescriptionPadding)
        {
            this._cmdName = commandName;
        }

        override protected void WriteUsage(ModelHelp modelHelp, TextWriter writer)
        {
            var values = modelHelp.Members.Where(m => m.OrdinalIndex.HasValue && m.OrdinalIndex != 0)
                .OrderBy(m => m.OrdinalIndex.Value)
                .Select(m => m.Name)
                .Concat(modelHelp.Members
                    .Where(m => m.OrdinalIndex.HasValue == false)
                    .Select(m => String.Format("[{0}]", String.Join("|", m.Switches.Select(s => modelHelp.SwitchDelimiter + s).ToArray()))));

            var dictionary = new Dictionary<string, string>
            {
                {this._cmdName, String.Join(" ", values.ToArray())}
            };

            WriteJustifiedOutput(dictionary, CommandSamplePadding);
        }

    }
}
