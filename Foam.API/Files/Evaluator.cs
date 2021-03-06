﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotNetCommons;
using Foam.API.Exceptions;

namespace Foam.API.Files
{
    public static class Evaluator
    {
        private const int TokenText = 0;
        private const int TokenVarStart = 1;
        private const int TokenVarEnd = 2;

        private static readonly List<string> Reserved = new List<string>
        {
            "filename",
            "filename-noext",
            "fileext",
            "filesize",
            "filedate",
            "filecrc",
            "date",
            "today",
            "time",
            "now",
            "utcdate",
            "utctoday",
            "utctime",
            "utcnow"
        };

        private static readonly StringTokenizer Parser = new StringTokenizer(new TokenDefinition[]
        {
            new TokenCharacterModeDefinition(TokenMode.Any, TokenText, false),
            new TokenStringDefinition("{@", TokenVarStart, false),
            new TokenStringDefinition("}", TokenVarEnd, false)
        });

        public static string Text(string text, FileItem file, Variables constants)
        {
            var list = Parser.Tokenize(text);
            if (list == null || list.Count == 0)
                return null;

            var inVar = false;
            var result = new StringBuilder();
            foreach (var token in list)
                switch (token.Value)
                {
                    case TokenText:
                        result.Append(inVar ? Variable(token.Text.Trim(), file, constants) : token.Text);
                        break;

                    case TokenVarStart:
                        if (inVar)
                            throw new FoamConfigurationException($"Invalid variable sequence in expression '{text}'");
                        inVar = true;
                        break;

                    case TokenVarEnd:
                        if (!inVar)
                            result.Append(token.Text);
                        else
                            inVar = false;
                        break;
                }

            return result.ToString();
        }

        public static string Variable(string variable, FileItem file, Variables constants)
        {
            if (string.IsNullOrWhiteSpace(variable))
                return null;

            switch (variable.ToLower())
            {
                case "filename":
                    return file?.Name;

                case "filename-noext":
                    return Path.GetFileNameWithoutExtension(file?.Name);

                case "fileext":
                    return Path.GetExtension(file?.Name);

                case "filesize":
                    return file?.Length.ToString();

                case "filedate":
                    return file?.Timestamp.ToString("yyyyMMdd-HHmmss");

                case "filecrc":
                    return file?.Crc32.ToString();

                case "date":
                case "today":
                    return DateTime.Today.ToString("yyyyMMdd");

                case "time":
                case "now":
                    return DateTime.Now.ToString("yyyyMMdd-HHmmss");

                case "utcdate":
                case "utctoday":
                    return DateTime.UtcNow.Date.ToString("yyyyMMdd");

                case "utctime":
                case "utcnow":
                    return DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

                default:
                    var result = file?.Variables.GetOrDefault(variable);
                    if (string.IsNullOrEmpty(result))
                        result = constants?.GetOrDefault(variable);
                    return result;
            }
        }

        public static bool IsReserved(string to)
        {
            return Reserved.Contains((to ?? "").ToLower());
        }
    }
}
