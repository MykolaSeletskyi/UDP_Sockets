using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _02_ChatClient
{
    public class MessageInfo
    {
        public enum TypeMessage { MyMessage,InfoMessage,TextMessage};
        public string User { get; set; }
        public string Time { get; set; }
        public string Text { get; set; }
        public TypeMessage @Type { get; set; }
        public override string ToString()
        {
            return @Type.ToString();
        }
    }
}
