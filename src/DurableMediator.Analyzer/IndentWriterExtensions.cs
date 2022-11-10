using System;
using System.CodeDom.Compiler;

namespace DurableMediator.Analyzer
{
    public static class IndentWriterExtensions
    {
        public static IDisposable NoIndent(this IndentedTextWriter indentWriter) => new IndentNothing();
        public static IDisposable Indent(this IndentedTextWriter indentWriter) => new IndentDisposable(indentWriter);
        public static IDisposable Braces(this IndentedTextWriter indentWriter) => new IndentDisposable(indentWriter, "{", "}");
        public static IDisposable ClassSetters(this IndentedTextWriter indentWriter) => new IndentDisposable(indentWriter, "{", "};");
        private class IndentDisposable : IDisposable
        {
            private readonly IndentedTextWriter _indentWriter;
            private readonly string? _after;

            public IndentDisposable(IndentedTextWriter indentWriter, string? before = null, string? after = null)
            {
                _indentWriter = indentWriter;
                _after = after;
                if (before != null)
                {
                    _indentWriter.WriteLine(before);
                }
                _indentWriter.Indent++;
            }

            public void Dispose()
            {
                _indentWriter.Indent--;
                if (_after != null)
                {
                    _indentWriter.WriteLine(_after);
                }
            }
        }

        private class IndentNothing : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
