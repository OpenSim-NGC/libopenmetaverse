
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OpenMetaverse;

namespace LitJson
{
    public class JsonOSUTF8Writer
    {
        #region Fields
        private static NumberFormatInfo number_format;

        private WriterContext context;
        private Stack<WriterContext> ctx_stack;
        private bool has_reached_end;
        private byte[] hex_seq;
        private int indentation;
        private int indent_value;
        private osUTF8 sb;
        private bool ownsb;
        private bool pretty_print;
        private bool validate;

        #endregion

        #region Properties
        public int IndentValue
        {
            get { return indent_value; }
            set
            {
                indentation = (indentation / indent_value) * value;
                indent_value = value;
            }
        }

        public bool PrettyPrint
        {
            get { return pretty_print; }
            set { pretty_print = value; }
        }

        public bool Validate
        {
            get { return validate; }
            set { validate = value; }
        }
        #endregion

        #region Constructors
        static JsonOSUTF8Writer()
        {
            number_format = NumberFormatInfo.InvariantInfo;
        }

        public JsonOSUTF8Writer()
        {
            sb = OSUTF8Cached.Acquire();
            ownsb = true;
            Init();
        }

        public JsonOSUTF8Writer(osUTF8 sb)
        {
            this.sb = sb;
            ownsb = false;
            Init();
        }
        #endregion


        #region Private Methods
        private void DoValidation(Condition cond)
        {
            if (!context.ExpectingValue)
                context.Count++;

            if (!validate)
                return;

            if (has_reached_end)
                throw new JsonException(
                    "A complete JSON symbol has already been written");

            switch (cond)
            {
                case Condition.InArray:
                    if (!context.InArray)
                        throw new JsonException(
                            "Can't close an array here");
                    break;

                case Condition.InObject:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException(
                            "Can't close an object here");
                    break;

                case Condition.NotAProperty:
                    if (context.InObject && !context.ExpectingValue)
                        throw new JsonException(
                            "Expected a property");
                    break;

                case Condition.Property:
                    if (!context.InObject || context.ExpectingValue)
                        throw new JsonException(
                            "Can't add a property here");
                    break;

                case Condition.Value:
                    if (!context.InArray &&
                        (!context.InObject || !context.ExpectingValue))
                        throw new JsonException(
                            "Can't add a value here");

                    break;
            }
        }

        private void Init()
        {
            has_reached_end = false;
            hex_seq = new byte[4];
            indentation = 0;
            indent_value = 4;
            pretty_print = false;
            validate = true;

            ctx_stack = new Stack<WriterContext>();
            context = new WriterContext();
            ctx_stack.Push(context);
        }

        private void Indent()
        {
            if (pretty_print)
                indentation += indent_value;
        }


        private void Put(string str)
        {
            if (pretty_print && !context.ExpectingValue)
                for (int i = 0; i < indentation; i++)
                    sb.AppendASCII(' ');

            sb.AppendASCII(str);
        }

        private void Put(char c)
        {
            if (pretty_print && !context.ExpectingValue)
                for (int i = 0; i < indentation; i++)
                    sb.AppendASCII(' ');

            sb.AppendASCII(c);
        }

        private void PutNewline()
        {
            PutNewline(true);
        }

        private void PutNewline(bool add_comma)
        {
            if (add_comma && !context.ExpectingValue &&
                context.Count > 1)
                sb.AppendASCII(',');

            if (pretty_print && !context.ExpectingValue)
                sb.AppendASCII('\n');
        }

        private void PutString(string str)
        {
            if (pretty_print && !context.ExpectingValue)
                for (int i = 0; i < indentation; i++)
                    sb.AppendASCII(' ');

            sb.AppendASCII('"');
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\n':
                        sb.AppendASCII("\\n");
                        break;

                    case '\r':
                        sb.AppendASCII("\\r");
                        break;

                    case '\t':
                        sb.AppendASCII("\\t");
                        break;

                    case '"':
                    case '\\':
                        sb.AppendASCII('\\');
                        sb.AppendASCII(c);
                        break;

                    case '\f':
                        sb.AppendASCII("\\f");
                        break;

                    case '\b':
                        sb.AppendASCII("\\b");
                        break;

                    default:
                        // Default, turn into a \uXXXX sequence
                        if (c >= 32 && c <= 126)
                            sb.AppendASCII(c);
                        else
                        {
                            sb.AppendASCII("\\u");
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 12)));
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 8)));
                            sb.Append(Utils.NibbleToHexUpper((byte)(c >> 4)));
                            sb.Append(Utils.NibbleToHexUpper((byte)c));
                        }
                        break;
                }
            }
            sb.AppendASCII('"');
        }

        private void Unindent()
        {
            if (pretty_print)
                indentation -= indent_value;
        }
        #endregion

        public override string ToString()
        {
            if (sb == null)
                return String.Empty;
            if (ownsb)
            {
                string ret = sb.ToString();
                OSUTF8Cached.Release(sb);
                return ret;
            }
            return sb.ToString();
        }

        public byte[] ToBytes()
        {
            if (sb == null)
                return new byte[0];
            if (ownsb)
                return OSUTF8Cached.GetArrayAndRelease(sb);
            return sb.ToArray();
        }

        public void Reset()
        {
            has_reached_end = false;

            ctx_stack.Clear();
            context = new WriterContext();
            ctx_stack.Push(context);

            if (sb != null)
                sb.Clear();
        }

        public void Write(bool boolean)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(boolean ? "true" : "false");

            context.ExpectingValue = false;
        }

        public void Write(decimal number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        public void Write(double number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            string str = Convert.ToString(number, number_format);
            Put(str);

            if (str.IndexOfAny(new char[] { '.', 'E' }) == -1)
                sb.AppendASCII(".0");

            context.ExpectingValue = false;
        }

        public void Write(int number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        public void Write(long number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        public void Write(string str)
        {
            DoValidation(Condition.Value);
            PutNewline();

            if (str == null)
                Put("null");
            else
                PutString(str);

            context.ExpectingValue = false;
        }

        public void Write(ulong number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        public void WriteArrayEnd()
        {
            DoValidation(Condition.InArray);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put(']');
        }

        public void WriteArrayStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put('[');

            context = new WriterContext();
            context.InArray = true;
            ctx_stack.Push(context);

            Indent();
        }

        public void WriteObjectEnd()
        {
            DoValidation(Condition.InObject);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
                has_reached_end = true;
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put('}');
        }

        public void WriteObjectStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put('{');

            context = new WriterContext();
            context.InObject = true;
            ctx_stack.Push(context);

            Indent();
        }

        public void WritePropertyName(string property_name)
        {
            DoValidation(Condition.Property);
            PutNewline();

            PutString(property_name);

            if (pretty_print)
            {
                if (property_name.Length > context.Padding)
                    context.Padding = property_name.Length;

                for (int i = context.Padding - property_name.Length;
                     i >= 0; i--)
                    sb.AppendASCII(' ');

                sb.AppendASCII(": ");
            }
            else
                sb.AppendASCII(':');

            context.ExpectingValue = true;
        }
    }
}
