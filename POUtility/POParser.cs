using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Streamlet.POManager.POUtility
{
    using StringPair = KeyValuePair<string, string>;
    using StringCollection = Dictionary<string, string>;

    ////////////////////////////////////////////////////////////////////////////////
    //
    // Public Exceptions
    //

    public class POException : Exception
    {
        public int Line
        {
            get;
            set;
        }

        public int Column
        {
            get;
            set;
        }

        public POException(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }

    public class TokenRequiredException : POException
    {
        public string Token
        {
            get;
            set;
        }

        public TokenRequiredException(string token, int line, int column)
            : base(line, column)
        {
            Token = token;
        }
    }

    public class ItemDuplicateException : POException
    {
        public ItemDuplicateException(int line, int column) :
            base(line, column)
        {
        }
    }

    public class HeaderFormatException : POException
    {
        public HeaderFormatException(int line, int column) :
            base(line, column)
        {
        }
    }

    //
    // Public Exceptions End
    //
    ////////////////////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// POParser for internal use.
    /// Usage:
    ///     POParser.Eval(poText)
    /// It will return a POData if succeeded, or may throw any of the exceptions defined above.
    /// </summary>
    class POParser
    {
        string _textToParse = null;
        int _currentPosition = 0;
        int _currentLine = 0;
        int _currentColumn = 0;

        static public POData Eval(string poText)
        {
            try
            {
                POParser parser = new POParser();
                parser._textToParse = poText;
                return parser.ParseData();
            }
            catch (POParser.TokenRequiredException e)
            {
                string token = "";

                switch (e.TokenExpected)
                { 
                    case TokenName.MessageId:
                        token = "msgid";
                        break;
                    case TokenName.MessageString:
                        token = "msgstr";
                        break;
                    case TokenName.Quote:
                        token = "\"";
                        break;
                    case TokenName.Character:
                    case TokenName.End:
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(token))
                { 
                    throw new POUtility.TokenRequiredException(token, e.Line, e.Column);
                }
            }
            catch (POParser.ItemDuplicateException e)
            {
                throw new POUtility.ItemDuplicateException(e.Line, e.Column);
            }
            catch (POParser.HeaderFormatException e)
            {
                throw new POUtility.HeaderFormatException(e.Line, e.Column);
            }
            catch (POParser.POException e)
            {
                throw new POUtility.POException(e.Line, e.Column);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //
        // Internal Exceptions
        //


        class POException : Exception
        {
            public int Line
            {
                get;
                set;
            }

            public int Column
            {
                get;
                set;
            }

            public POException(int line, int column)
            {
                Line = line;
                Column = column;
            }
        }

        class TokenRequiredException : POException
        {
            public TokenName TokenExpected
            {
                get;
                set;
            }

            public TokenRequiredException(TokenName tokenExpected, int line, int column)
                : base(line, column)
            {
                TokenExpected = tokenExpected;
            }
        }

        class ItemDuplicateException : POException
        {
            public ItemDuplicateException(int line, int column) :
                base(line, column)
            {
            }
        }

        class HeaderFormatException : POException
        {
            public HeaderFormatException(int line, int column) :
                base(line, column)
            {
            }
        }

        //
        // Internal Exceptions End
        //
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////
        //
        // Grammar
        //

        //
        // POData = POItem *
        // POItem = MessageId String MessageString String
        // String = ( Quote OriginalString Quote ) +
        //

        POData ParseData()
        {
            POData poData = new POData();

            while (LookAhead().Name != TokenName.End)
            {
                var item = ParseItem();

                if (item.Key == "")
                {
                    if (poData.Header.Count != 0)
                    {
                        throw new ItemDuplicateException(_currentLine, _currentColumn);
                    }
                    else
                    {
                        poData.Header = ParseHeader(item.Value);
                    }
                }
                else
                {
                    if (poData.Content.Keys.Contains(item.Key))
                    {
                        throw new ItemDuplicateException(_currentLine, _currentColumn);
                    }
                    else
                    {
                        poData.Content.Add(item.Key, item.Value);
                    }
                }
            }

            return poData;
        }

        StringCollection ParseHeader(string str)
        {
            StringCollection header = new StringCollection();

            var lines = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var headerEntry = line.Split(new string[] { ": ", ":" }, 2, StringSplitOptions.None);

                if (headerEntry.Count() != 2)
                {
                    throw new HeaderFormatException(_currentLine, _currentColumn);
                }

                header.Add(headerEntry[0], headerEntry[1]);
            }

            return header;
        }

        StringPair ParseItem()
        {
            var tokenMessageId = LookAhead();

            if (tokenMessageId.Name != TokenName.MessageId)
            {
                throw new TokenRequiredException(TokenName.MessageId, _currentLine, _currentColumn);
            }

            Match(tokenMessageId);

            var messageId = ParseString();

            var tokenMessageString = LookAhead();

            if (tokenMessageString.Name != TokenName.MessageString)
            {
                throw new TokenRequiredException(TokenName.MessageString, _currentLine, _currentColumn);
            }

            Match(tokenMessageString);

            var messageString = ParseString();

            return new StringPair(messageId, messageString);
        }

        string ParseString()
        {
            var tokenQuote = LookAhead();

            if (tokenQuote.Name != TokenName.Quote)
            {
                throw new TokenRequiredException(TokenName.Quote, _currentLine, _currentColumn);
            }

            Match(tokenQuote);

            var str = ParseOriginalString();

            tokenQuote = LookAhead();

            if (tokenQuote.Name != TokenName.Quote)
            {
                throw new TokenRequiredException(TokenName.Quote, _currentLine, _currentColumn);
            }

            Match(tokenQuote);

            while ((tokenQuote = LookAhead()).Name == TokenName.Quote)
            {
                Match(tokenQuote);

                var strAppend = ParseOriginalString();

                tokenQuote = LookAhead();

                if (tokenQuote.Name != TokenName.Quote)
                {
                    throw new TokenRequiredException(TokenName.Quote, _currentLine, _currentColumn);
                }

                Match(tokenQuote);

                str += strAppend;
            }

            return str;
        }

        string ParseOriginalString()
        {
            string str = "";

            for (Token token = LookAhead(false); token.Name == TokenName.Character; Match(token), token = LookAhead(false))
            {
                str += token.Content;
            }

            return str;
        }
        
        //
        // Grammar
        //
        ////////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////////
        //
        // Lexical
        //

        enum TokenName
        {
            Quote,
            MessageId,
            MessageString,
            Character,
            End
        }

        class Token
        {
            public TokenName Name
            {
                get;
                set;
            }

            public int Length
            {
                get;
                set;
            }

            public string Content
            {
                get;
                set;
            }

            public Token(TokenName name, string content)
            {
                Name = name;
                Content = content;
                Length = Content.Length;
            }

            public Token(TokenName name, string content, int length)
            {
                Name = name;
                Content = content;
                Length = length;
            }
        }

        List<Token> _constTokens = new List<Token>
        {
            new Token(TokenName.Quote, "\""),
            new Token(TokenName.MessageId, "msgid"),
            new Token(TokenName.MessageString, "msgstr")
        };

        void Match(Token token)
        {
            _currentPosition += token.Length;
        }

        void IgnoreSpace()
        {
            while (true)
            {
                if (_currentPosition >= _textToParse.Length)
                {
                    return;
                }

                switch (_textToParse[_currentPosition])
                {
                    case '\r':
                        if (_currentPosition + 1 < _textToParse.Length && _textToParse[_currentPosition + 1] != '\n')
                        {
                            ++_currentLine;
                        }
                        ++_currentPosition;
                        break;
                    case '\n':
                        ++_currentLine;
                        ++_currentPosition;
                        break;
                    case ' ':
                    case '\t':
                        ++_currentPosition;
                        break;
                    case '#':
                        IgnoreComment();
                        break;
                    default:
                        return;
                }
            }
        }

        void IgnoreComment()
        {
            if (_textToParse[_currentPosition] != '#')
            {
                return;
            }

            while (++_currentPosition < _textToParse.Length)
            {
                if (_textToParse[_currentPosition] == '\r')
                {
                    if (_currentPosition + 1 < _textToParse.Length && _textToParse[_currentPosition + 1] == '\n')
                    {
                        ++_currentPosition;
                    }

                    ++_currentPosition;

                    return;
                }

                if (_textToParse[_currentPosition] == '\n')
                {
                    ++_currentPosition;

                    return;
                }
            }
        }

        Token LookAhead(bool ignoreSpace = true)
        {
            if (ignoreSpace)
            {
                IgnoreSpace();
            }

            if (_currentPosition >= _textToParse.Length)
            {
                return new Token(TokenName.End, "");
            }

            foreach (Token token in _constTokens)
            {
                if (_currentPosition + token.Length <= _textToParse.Length &&
                    _textToParse.Substring(_currentPosition, token.Length) == token.Content)
                {
                    return token;
                }
            }

            if (_textToParse[_currentPosition] == '\\' && _currentPosition + 1 < _textToParse.Length)
            {
                switch (_textToParse[_currentPosition + 1])
                {
                    case '\\':
                        return new Token(TokenName.Character, "\\", 2);
                    case '\"':
                        return new Token(TokenName.Character, "\"", 2);
                    case 'r':
                        return new Token(TokenName.Character, "\r", 2);
                    case 'n':
                        return new Token(TokenName.Character, "\n", 2);
                    case 't':
                        return new Token(TokenName.Character, "\t", 2);
                    default:
                        break;
                }
            }

            return new Token(TokenName.Character, _textToParse.Substring(_currentPosition, 1));
        }

        //
        // Lexical End
        //
        ////////////////////////////////////////////////////////////////////////////////
    }
}
