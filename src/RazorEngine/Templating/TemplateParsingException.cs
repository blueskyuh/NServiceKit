﻿namespace RazorEngine.Templating
{
    using System;
    using System.Web.Razor.Parser.SyntaxTree;

    /// <summary>
    /// Defines an exception that occurs during parsing of a template.
    /// </summary>
    public class TemplateParsingException : Exception
    {
        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateParsingException"/>
        /// </summary>
        internal TemplateParsingException(RazorError error)
            : base(error.Message)
        {
            Column = error.Location.CharacterIndex;
            Line = error.Location.LineIndex;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the column the parsing error occured.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Gets the line the parsing error occured.
        /// </summary>
        public int Line { get; private set; }
        #endregion
    }
}