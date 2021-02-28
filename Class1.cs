using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace AppConsoleTC
{
    class Class1
    {
        public Class1()
        {
            string xml = @"
                <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                <soap:Body>
                <getSeed xmlns='https://palena.sii.cl/DTEWS/CrSeed.jws/'>
                </getSeed>
                </soap:Body>
                </soap:Envelope>
                ";
            xml = xml.Replace("'", "\"");
            string url = "https://palena.sii.cl/DTEWS/CrSeed.jws?WSDL";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/xml;charset=UTF-8";
            request.Headers.Add("SOAPAction", "");
            var data = System.Text.Encoding.ASCII.GetBytes(xml);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


            Console.WriteLine(responseString);
        }


    }
}
