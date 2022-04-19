﻿using NPreprocessor.Input;
using NPreprocessor.Output;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPreprocessor.Macros
{
    public class IfMacro : IMacro
    {
        public IfMacro(IMacroResolver macroResolver)
        {
            MacroResolver = macroResolver;
        }

        public string Pattern => "if";

        public bool AreArgumentsRequired => true;

        public IMacroResolver MacroResolver { get; }

        public async Task<(List<TextBlock> result, bool finished)> Invoke(ITextReader reader, State state)
        {
            var call = CallParser.GetInvocation(reader, 0, state.Definitions);
            reader.Current.Advance(call.length);
            var args = call.args;
            var expression = MacroString.Trim(args[0]);

            if (await IsExpressionTrue(expression, state))
            {
                return (MacroString.GetBlocks(args[1], state.NewLineEnding), false);
            }
            else
            {
                if (args.Length == 3)
                {
                    return (MacroString.GetBlocks(args[2], state.NewLineEnding), false);
                }
                else
                {
                    return (new List<TextBlock>(), true);
                }
            }
        }

        private async Task<bool> IsExpressionTrue(string expression, State state)
        {
            var expressionResolved = await MacroResolver.Resolve(new TextReader(expression), state);

            var result = CFGToolkit.ExpressionEvaluator.Evaluator.Eval(expressionResolved.FullText, new Dictionary<string, object>(), CFGToolkit.ExpressionParser.ExpressionLanguage.C);

            if (result is bool rb)
            {
                return rb;
            }

            if (result is int ri)
            {
                return ri > 0;
            }

            if (result is double rd)
            {
                return rd > 0.0;
            }

            return false;
        }
    }
}
