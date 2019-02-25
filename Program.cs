using System;
using RestSharp;
using Polly;
using System.Json;
using System.Threading.Tasks;
using System.Net;

namespace wecode
{
    class Program {

        static public string login (RestClient client, string userName ) {
            string uid = string.Empty;
            
            var request = new RestRequest(String.Format("v1/register?userName={0}", userName), Method.POST);

            request.ReadWriteTimeout = 400;
            request.Timeout = 1000;
            try{
                
                IRestResponse response = client.Execute(request);
                Console.WriteLine (response.StatusCode.ToString());
                System.Json.JsonObject result = JsonValue.Parse(response.Content) as System.Json.JsonObject; 

                if (result!= null && result.Keys.Contains("user")){
                    Console.WriteLine("Login OK");
                    return result["user"];
                }
            }catch(Exception) {
                return string.Empty;
            }
            return String.Empty;
        }

        static public Tuple<string, int> excavate(IRestClient client) {

            Console.WriteLine("EXCAVATE!");
            var request = new RestRequest(String.Format("v1/excavate"), Method.POST);
            request.Timeout = 500;
            request.ReadWriteTimeout = 400;

            Policy retryPolicy = Policy.Handle<Exception>().Retry(3);

            try {
                System.Json.JsonObject result = retryPolicy.Execute ( () => {
                    IRestResponse response = client.Execute(request);
                    return JsonValue.Parse(response.Content);
                } ) as System.Json.JsonObject;
                if (result!= null && result.Keys.Contains("bucketId") && result.Keys.Contains("gold") )    
                {
                    return new Tuple<string, int> (result["bucketId"],result["gold"]["units"]);
                }
                }catch(Exception ) {
                    return null;
                } 
            
            return null;
        }

        static public void reclamar (IRestClient client, string userId, Tuple<String, int> bucketInfo) {
            Console.WriteLine("RECLAMAR!");
            var request = new RestRequest(String.Format(
                "v1/store?userId={0}&bucketId={1}", userId, bucketInfo.Item1
                )
                , Method.POST);

            request.Timeout = 500;
            request.ReadWriteTimeout = 400;
            Policy timeoutPolicy = Policy.Timeout(1);

            Policy retryPolicy = Policy.Handle<Exception>().Retry(3);

            string result = retryPolicy.Execute (
             ()=>timeoutPolicy.Execute ( () => {
                IRestResponse response = client.Execute(request);
                Console.WriteLine (userId + " " + response.StatusCode.ToString() + " Quantity " + bucketInfo.Item2);
                return response.Content;
                } ));

        }
        
        static public void scrapLoop (IRestClient client, string loginId) {
            for(;;) {
                Tuple<String, int> escavado = excavate(client);
                if (escavado!= null && escavado.Item2>0){
                    reclamar(client, loginId, escavado);
                }
            }
        }

        static public void scrapper (string _arg, string username) {
            RestClient client = new RestClient(_arg);

            string loginId = String.Empty;
            while (loginId == String.Empty || loginId == null){
                loginId = login (client, username);
            }

            Console.WriteLine(loginId);

            Parallel.For(0, Int64.MaxValue,new ParallelOptions{MaxDegreeOfParallelism=5},
                (i)=>{scrapLoop(client, loginId);}
            );

        }
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 50;

            scrapper("http://ganger85.herokuapp.com", "dddddddd");
        }
    }
}
