using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace XmlParser
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("Example.xml");
            var nsmgr = new XmlNamespaceManager(xDoc.NameTable);
            nsmgr.AddNamespace("cim", "http://iec.ch/TC57/2014/CIM-schema-cim16#");
            XmlElement xRoot = xDoc.DocumentElement;
            
            XmlNodeList childnodes = xRoot.SelectNodes("*[local-name()='Substation']");
            List<Substation> sub = new List<Substation>();
            foreach (XmlNode n in childnodes)
            {
                sub.Add(new Substation(n.Attributes[0].InnerText,
                                       n.SelectSingleNode("*[local-name()='IdentifiedObject.name']").InnerText,
                                       null));
                //Console.WriteLine(n.Attributes[0].InnerText);
                //Console.WriteLine(n.SelectSingleNode("*[local-name()='IdentifiedObject.name']").InnerText);
            }
            childnodes = xRoot.SelectNodes("*[local-name()='VoltageLevel']");
            foreach (XmlNode n in childnodes)
            {
                Substation s = sub.Where(x => x.id.Equals(n.SelectSingleNode("*[local-name()='VoltageLevel.Substation']").Attributes[0].InnerText)).Select(x => x).First();
                s.vl = new VoltageLevel(n.Attributes[0].InnerText,
                                      n.SelectSingleNode("*[local-name()='IdentifiedObject.name']").InnerText,
                                      new List<SynchronousMachine>());
                //Console.WriteLine(n.Attributes[0].InnerText);
                //Console.WriteLine(n.SelectSingleNode("*[local-name()='IdentifiedObject.name']").InnerText);
            }
            childnodes = xRoot.SelectNodes("*[local-name()='SynchronousMachine']");
            foreach (XmlNode n in childnodes)
            {
                try
                {
                    VoltageLevel vl = sub.Where(x => 
                    {
                        if (x.vl != null)
                        {
                            return x.vl.id.Equals(n.SelectSingleNode("*[local-name()='Equipment.EquipmentContainer']").Attributes[0].InnerText);
                        }
                        else
                        {
                            return false;
                        }
                    }).Select(x => x.vl).First();
                    vl.smList.Add(new SynchronousMachine(n.Attributes[0].InnerText,n.SelectSingleNode("*[local-name()='IdentifiedObject.name']").InnerText));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception while parsing " + n.Attributes[0].InnerText);
                    Console.WriteLine(e.Message);
                }
            }
            string jsonFile = "";
            foreach(Substation s in sub)
            {
                jsonFile+= JsonConvert.SerializeObject(s)+"\r\n";
            }
            Console.WriteLine(jsonFile);
            File.WriteAllText("jsonFile.json", jsonFile);
        }
    }

    class Substation
    {
        [JsonIgnore]
        public string id;
        public string substationName;
        public VoltageLevel vl;

        public Substation(string id, string substationName, VoltageLevel vl)
        {
            this.id = id;
            this.substationName = substationName;
            this.vl = vl;
        }
    }
    class VoltageLevel
    {
        [JsonIgnore]
        public string id;
        public string voltageLevelName;
        public List<SynchronousMachine> smList;

        public VoltageLevel(string id, string voltageLevelName, List<SynchronousMachine> smList)
        {
            this.id = id;
            this.voltageLevelName = voltageLevelName;
            this.smList = smList;
        }
    }
    class SynchronousMachine
    {
        [JsonIgnore]
        public string id;
        public string synchronousMachineName;

        public SynchronousMachine(string id, string synchronousMachineName)
        {
            this.id = id;
            this.synchronousMachineName = synchronousMachineName;
        }
    }


}
