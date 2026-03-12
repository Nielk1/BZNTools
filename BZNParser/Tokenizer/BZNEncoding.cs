using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BZNParser.Tokenizer
{
    static class BZNEncoding
    {
        public static Encoding win1252;
        static Encoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            win1252 = Encoding.GetEncoding(1252);
        }
    }
}
