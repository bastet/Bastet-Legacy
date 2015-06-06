using Bastet.Database.Model;
using RestSharp;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bastet.Backends.Http.Basic
{
    public class BasicHttpBackend
        : BaseHttpBackend
    {
        public override async Task<BackendResponse> Request(IDbConnection db, Device device, BackendRequest request)
        {
            //Make the request
            var client = new RestClient();
            var response = await client.ExecuteTaskAsync(await ToRestSharpRequest(request));

            //Convert the response back
            return await FromRestSharpResponse(response);
        }

        private static async Task<IRestRequest> ToRestSharpRequest(BackendRequest request)
        {
            //Create the request
            IRestRequest req = new RestRequest(new Uri(request.Url), (Method)Enum.Parse(typeof(Method), request.Method));

            //Add all the headers
            foreach (var header in request.Headers)
                req.AddHeader(header.Key, header.Value);

            //Copy the body
            if (request.Body != null)
            {
                MemoryStream m = new MemoryStream();
                await request.Body.CopyToAsync(m);
                req.AddParameter("body", m.ToArray(), ParameterType.RequestBody);
            }

            return req;
        }

        private static Task<BackendResponse> FromRestSharpResponse(IRestResponse response)
        {
            return Task.Factory.StartNew(() => new BackendResponse(
                response.StatusCode,
                response.Headers.Select(a => new KeyValuePair<string, string>(a.Name, a.Value.ToString())),
                new MemoryStream(response.RawBytes)
            ));
        }

        public override Task<BackendDescription> Describe(IDbConnection db, Device device)
        {
            throw new NotImplementedException();
        }

        public override void Setup(Database.Database database)
        {
            using (var db = database.ConnectionFactory.Open())
                db.CreateTableIfNotExists<BasicHttpModel>();
        }
    }
}
