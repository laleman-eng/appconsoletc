using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace AppConsoleTC
{
    public class Controlador
    {
        IMongoCollection<BsonDocument> collection;
        int auxiliarRecursivo = 0;
        int diasParam;

        public Controlador()
        {
           
        }

        public void init()
        {
            inicializacion(false,"","");
        }

        public void inicializacion (bool recursivo, string code, string serieAux)
        {
            try
            {
                if (recursivo)
                    auxiliarRecursivo--; 


                Log.save("Iniciando proceso");
                int contador = 0;
                Param list;
                String xml;
                string jsonPath = @"params/params.json";
                using (StreamReader jsonStram = File.OpenText(jsonPath))
                {
                    var json = jsonStram.ReadToEnd();
                    list = JsonConvert.DeserializeObject<Param>(json);
                }
                string url = list.Url;
                diasParam = list.dias;

                var client = new MongoClient(list.stringConection);
                var database = client.GetDatabase(list.database);
                collection = database.GetCollection<BsonDocument>(list.collection);

                DateTime date = DateTime.Now;
                date = date.AddDays(diasParam + auxiliarRecursivo);
                string dateFormat = date.ToString("yyyy-MM-dd");
                Log.save("Iniciando proceso descarga TC Fecha: " + dateFormat);
                
                if (!recursivo)
                {
                    foreach (Series serie in list.Series)
                    {
                        Log.save("Codigo: " + serie.codigo);
                        contador++;
                        if (contador == 4)
                        {
                            Thread.Sleep(1000);
                            contador = 0;
                        }
                        xml = getXmlWS(dateFormat, dateFormat, serie.serie);
                        wsIndicadorBC(xml, url, serie.codigo, dateFormat, serie.descripcion, serie.serie );
                    }
                }
                else
                {
                    xml = getXmlWS(dateFormat, dateFormat, serieAux);
                    wsIndicadorBC(xml, url, code, dateFormat, "Recursivo", serieAux);
                }    

                Log.save("Fin del proceso");
            }
            catch (Exception ex)
            {
                Log.save(this, ex);
            }
        }

        private String getXmlWS(string firstDate, string lastDate , string serie)
        {
            try
            {
                string xmlPath = @"params/doc.xml";
                XmlDocument xml = new XmlDocument();
                xml.Load(xmlPath);

                XmlNodeList nodo;

                nodo = xml.GetElementsByTagName("firstDate");

                foreach (XmlNode element in nodo)
                {
                    element.InnerText = firstDate;
                }

                nodo = xml.GetElementsByTagName("lastDate");

                foreach (XmlNode element in nodo)
                {
                    element.InnerText = lastDate;
                }

                nodo = xml.GetElementsByTagName("string");
                foreach (XmlNode element in nodo)
                {
                    element.InnerText = serie;
                }

                xml.Save(@"params/doc3.xml");
                return xml.InnerXml;
            }
            catch (Exception ex)
            {
                Log.save(this, ex);
                return "";
            }
     
          
           
        }

        public void wsIndicadorBC(string xml, string url, string codigo, string fecha, string descripcion, string serie)
        {
            try
            {
                //xml = xml.Replace("'", "\"");
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "text/xml";
                var data = System.Text.Encoding.ASCII.GetBytes(xml);
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Log.save("Extrayendo datos peticion POST: ");
                getResultXML(responseString, codigo,fecha,descripcion, serie);
            }
            catch (Exception ex)
            {
                Log.save(this, ex);
            }

        }


        public void getResultXML(string responseString,string codigo,string fecha, string descripcion, string serie)
        {
            try
            {
                XmlDocument docXml = new XmlDocument();
                docXml.LoadXml(responseString);
                XmlNodeList nodo;
                TipoCambio tc = new TipoCambio();
                nodo = docXml.GetElementsByTagName("obs");

                if (nodo.Count == 1)  //trae registros del BC
                {
                    foreach (XmlNode element in nodo)
                    {
                        if (element.SelectSingleNode("statusCode").InnerText == "OK")
                        {
                            if (descripcion == "Recursivo")
                            {
                                DateTime date = DateTime.Now;
                                date = date.AddDays(diasParam);
                                string dateFormat = date.ToString("yyyy-MM-dd");
                                tc.fecha = dateFormat;
                            }
                            else
                            {
                                tc.fecha = fecha;
                            }
                            tc.descripcion = descripcion;
                            tc.valor = double.Parse(element.SelectSingleNode("value").InnerText);
                            tc.serie = element.SelectSingleNode("seriesKey/seriesId").InnerText;
                            tc.codigo = codigo;
                            Log.save("Resultado: OK valor: "+ tc.valor);
                            insetarDocumentoBD(tc);
                        }
                        else
                        {
                            Log.save("Resultado WS Banco Central con error: " + responseString);
                        }
                    }
                }
                else  //asumiendo que es feriado invoco con -1
                {
                    inicializacion(true, codigo, serie);
                    auxiliarRecursivo = 0;

                    //string xmlOld;
                    //int dataDateValueOld = -1;
                    //DateTime date = DateTime.Now;
                    //date = date.AddDays(dataDateValueOld);
                    //string dateFormat = date.ToString("yyyy-MM-dd");
                    //xmlOld = getXmlWS(dateFormat, dateFormat, codigo);
                    //wsIndicadorBC(xmlOld, url, codigo, dateFormat, serie.descripcion)
                }
            }
            catch (Exception ex)
            {
                Log.save(this, ex);
            }
        }

        public void insetarDocumentoBD(TipoCambio tipoCambio)
        {
            try
            {
                string text = JsonConvert.SerializeObject(tipoCambio);
                var document = BsonSerializer.Deserialize<BsonDocument>(text);
                collection.InsertOne(document);
                Log.save("Documento Insertado en BD");
            }
            catch (Exception ex)
            {
                Log.save(this, ex);
            }
        }


    }
}
