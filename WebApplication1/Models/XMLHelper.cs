using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;

namespace WebApplication1.Models {
    public class XMLHelper {

        public T GetXmlRequest<T>(string requestUrl) {
            try {
                requestUrl = "http://"+requestUrl;
                WebRequest apiRequest = WebRequest.Create(requestUrl);
                HttpWebResponse apiResponse = (HttpWebResponse)apiRequest.GetResponse();

                if (apiResponse.StatusCode == HttpStatusCode.OK) {
                    string xmlOutput;
                    using (StreamReader sr = new StreamReader(apiResponse.GetResponseStream()))
                        xmlOutput = sr.ReadToEnd();

                    XmlSerializer xmlSerialize = new XmlSerializer(typeof(T));

                    var xmlResult = (T)xmlSerialize.Deserialize(new StringReader(xmlOutput));

                    if (xmlResult != null)
                        return xmlResult;
                    else
                        return default(T);
                }
                else {
                    return default(T);
                }
            }
            catch (Exception ex) {
                // Log error here.
                System.Diagnostics.Debug.WriteLine(ex);
                return default(T);
            }
        }
    }
}