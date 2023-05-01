// See https://aka.ms/new-console-template for more information
using ContinuousInterop.Cli;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Headers;
using static System.Net.WebRequest;
using System.Reflection.PortableExecutable;

//try just in case something went wrong whith calling the api
try
{
    //Use using so that if the code end the client disposes it self
    using (HttpClient client = new HttpClient())
    {
        //Setup authentication information
        string yourusername = "aengland@ea.com";
        string yourpwd = "11eeeef96eb31f3541c206bfb0145b510a";
        //this is when you expect json to return from the api
        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //add the authentication to the request
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(
                System.Text.ASCIIEncoding.ASCII.GetBytes($"{yourusername}:{yourpwd}")));
        //api link used to make the call
        var requestLink = $"https://eav-fbvs-fz09.eac.ad.ea.com/job/dev-na/job/Packages.Frostbite.Wpf/21894/logText/progressiveText?start=0";
        using (HttpResponseMessage response = await client.GetAsync(requestLink))
        {
            //Make sure the request was successfull before proceding
            response.EnsureSuccessStatusCode();
            //Get response from website and convert to a string
            //string responseBody = await response.Content.ReadAsStringAsync();

            var handler = new LogHandler(new DotnetLogParser());

            var logContent = handler.ReadLog(new StreamReader(await response.Content.ReadAsStreamAsync()));

            await handler.Send(logContent);
        }
    }
}
//Catch the exception if something went from and show it!
catch (Exception)
{
    throw;
}

//using var reader = new StreamReader("C:\\Code\\ContinuousInterop\\ContinuousInterop.Cli\\log2.txt");
