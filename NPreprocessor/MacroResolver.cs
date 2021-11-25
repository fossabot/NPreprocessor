﻿using NPreprocessor.Macros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NPreprocessor
{
    public class MacroResolver : IMacroResolver
    {
        private List<IMacro> _macros = new List<IMacro>();

        public MacroResolver()
        {
            _macros.Add(new LineCommentMacro());
            _macros.Add(new UndefineMacro());
            _macros.Add(new DefineMacro());
            _macros.Add(new DefineResolveMacro());
            _macros.Add(new IfDefMacro());
            _macros.Add(new IfNDefMacro());
            _macros.Add(new IncludeMacro());
            _macros.Add(new DnlMacro());
            _macros.Add(new IncrMacro());
            _macros.Add(new DecrMacro());
        }

        public List<IMacro> Macros { get => _macros; }


        public List<string> Resolve(ITextReader txtReader, State state = null)
        {
            if (state == null)
            {
                state = new State();
            }

            var result = new List<string>();
            List<string> lastResult = null;
            do
            {
                lastResult = ProcessCurrentLine(txtReader, state);
                if (lastResult != null)
                {
                    AddToResult(result, lastResult, state.MergePoints == 0);

                    if (state.MergePoints > 0)
                    {
                        state.MergePoints -= 1;
                    }
                }
            }
            while (lastResult != null);

            return result;
        }

        private List<string> ProcessCurrentLine(ITextReader txtReader, State state)
        {
            txtReader.MoveNext();
            var currentLine = txtReader.Current;
            if (currentLine == null) return null;
            var result = new List<string>();


            while (txtReader.Current?.Remainder != null && txtReader.Current?.Remainder != String.Empty)
            {
                (IMacro macro, int position) macroToCall = FindBestMacroToCall(txtReader, state);

                if (macroToCall.macro != null)
                {
                    if (!ProcessMacro(txtReader, state, result, macroToCall.macro, macroToCall.position))
                    {
                        break;
                    }
                }
                else
                {
                    AddToResult(result, new List<string> { txtReader.Current.Remainder }, false);
                    txtReader.Current.Finish();
                }
            }

            if (txtReader.Current?.Remainder == String.Empty)
            {
                result.Add(string.Empty);
                txtReader.Current.Finish();
            }

            return result;
        }

        private bool ProcessMacro(ITextReader txtReader, State state, List<string> result, IMacro macroToCall, int position)
        {
            state.Stack.Push(macroToCall);
            var before = txtReader.Current.Remainder;

            var skipped = before.Substring(0, position);
            txtReader.Current.Consume(position);

            var macroResult = macroToCall.Invoke(txtReader, state);

            var lines = macroResult.result;

            if (lines != null && lines.Count() > 0)
            {
                lines[0] = skipped + lines[0];

                if (lines.Count == 1 && lines[0] == String.Empty)
                {
                    if (state.CreateNewLine || !result.Any())
                    {
                        result.Add(string.Empty);
                    }
                    state.Stack.Pop();
                    return true;
                }

                if (!macroResult.finished)
                {
                    var cascadeTextReader = new TextReader(string.Join(state.NewLineEnding, lines), state.NewLineEnding);
                    var cascadeResult = Resolve(cascadeTextReader, new State() 
                    { 
                        Stack = new Stack<IMacro>(state.Stack), 
                        Definitions = state.Definitions,
                        Mappings = state.Mappings,
                        MappingsParameters = state.MappingsParameters
                    });
                    AddToResult(result, cascadeResult, state.CreateNewLine);
                }
                else
                {
                    AddToResult(result, lines, state.CreateNewLine);
                }
            }

            state.Stack.Pop();
            return true;
        }

        private static void AddToResult(List<string> result, List<string> toAdd, bool createNewLine)
        {
            if (!result.Any() || createNewLine)
            {
                result.AddRange(toAdd);
            }
            else
            {
                result[result.Count - 1] += toAdd.First();
                result.AddRange(toAdd.Skip(1));
            }
        }


        private (IMacro macro, int position) FindBestMacroToCall(ITextReader txtReader, State state)
        {
            IMacro bastStartMacro = FindStartingMacro(txtReader);
            if (bastStartMacro != null)
            {
                return (bastStartMacro, 0);
            }

            var bestMacro = _macros
                .Where(m => m.Pattern != null)
                .Select(macro => (macro, Regex.Match(txtReader.Current.Remainder, @"(?:\b|\W|\s)" + "(" + macro.Pattern + ")")))
                .Where(macroWithMatch => macroWithMatch.Item2.Success)
                .OrderBy(macroWithMatch => macroWithMatch.Item2.Groups[1].Index)
                .FirstOrDefault();

            if (bestMacro != default)
            {
                return (bestMacro.macro, bestMacro.Item2.Groups[1].Index);
            }

            // dynamic macros
            var bestDynamicMacro = _macros
               .OfType<IDynamicMacro>()
               .FirstOrDefault(macroWithMatch => macroWithMatch.CanBeInvoked(txtReader, state));

            return (bestDynamicMacro, 0);
        }

        private IMacro FindStartingMacro(ITextReader txtReader)
        {
            foreach (var macro in _macros.Where(m => m.Pattern != null))
            {
                if (Regex.IsMatch(txtReader.Current.Remainder, "^" + macro.Pattern))
                {
                    return macro;
                }
            }
            return null;
        }

    }
}
