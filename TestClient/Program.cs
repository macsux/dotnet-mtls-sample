using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;

var certBytes = File.ReadAllBytes("sample-cert.pfx");

HttpRequestMessage request;
HttpClient client;
while (true)
{
    while (true)
    {
        Console.WriteLine("Select one...");
        Console.WriteLine("1: Make HTTP call with cert in header (simulate request from GoRouter)");
        Console.WriteLine("2: Make HTTPS call with cert as part of handshake");
        var key = Console.ReadKey(true).KeyChar;
        Console.WriteLine();
        if (key == '1')
        {
            var base64Cert = Convert.ToBase64String(certBytes);
            request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost:5000/sample"));
            request.Headers.Add("X-Forwarded-Client-Cert", base64Cert);
            request.Headers.Add("X-Forwarded-Proto", "https");
            client = new HttpClient();
            break;
        }

        if (key == '2')
        {
            var cert = new X509Certificate2(certBytes);
            var httpHandler = new HttpClientHandler();
            httpHandler.ClientCertificates.Add(cert);
            httpHandler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true; // trust any cert presented by server - don't do this in prod
            client = new HttpClient(httpHandler);
            request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://localhost:5001/sample"));

            break;
        }

        Console.Error.WriteLine("Invalid selection");
    }

    try
    {
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        foreach (var line in await response.Content.ReadFromJsonAsync<List<string>>())
            Console.WriteLine(line);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }
}