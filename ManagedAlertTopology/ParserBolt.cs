using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.SCP;
using Microsoft.SCP.Rpc.Generated;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ManagedAlertTopology
{
    public class ParserBolt : ISCPBolt
    {
        Context _context;

        public ParserBolt(Context ctx)
        {
            this._context = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            // set output schemas
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(double), typeof(string), typeof(string) });

            // Declare input and output schemas
            _context.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));

            _context.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());
        }
        public void Execute(SCPTuple tuple)
        {
            string json = tuple.GetString(0);

            var node = JObject.Parse(json);
            var temp = node.GetValue("temp");
            JToken tempVal;

            if (node.TryGetValue("temp", out tempVal)) //assume must be a temperature reading
            {
                Context.Logger.Info("temp:" + temp.Value<double>());
                JToken createDate = node.GetValue("createDate");
                JToken deviceId = node.GetValue("deviceId");
                _context.Emit(Constants.DEFAULT_STREAM_ID, 
                    new List<SCPTuple>() { tuple }, 
                    new List<object> { tempVal.Value<double>(), createDate.Value<string>(), deviceId.Value<string>() });
            }

            _context.Ack(tuple);
        }

        public static ParserBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new ParserBolt(ctx);
        }
    }
}