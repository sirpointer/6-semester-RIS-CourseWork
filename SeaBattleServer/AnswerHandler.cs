using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;

namespace SeaBattleServer
{
    internal class AnswerHandler
    {
        public static string GetErrorMessage()
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Error));
            jObject.Add(JsonStructInfo.Result, "");

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }
    }
}
