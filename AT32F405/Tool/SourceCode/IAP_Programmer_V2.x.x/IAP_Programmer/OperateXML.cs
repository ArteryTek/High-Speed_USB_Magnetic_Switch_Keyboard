using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace IAP_Demo
{
    class OperateXML
    {
        public static XmlDocument CreateXMLFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "");//创建类型声明节点
            xmlDoc.AppendChild(node);
            XmlElement root = xmlDoc.CreateElement("Configuration");//创建根节点
            xmlDoc.AppendChild(root);
            return xmlDoc;
        }

        public static void AppendXMLNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }
        public static void AppendXMLElement(XmlDocument xmlDoc, XmlNode parentNode, string name)
        {
            XmlElement node = xmlDoc.CreateElement(name);
            parentNode.AppendChild(node);
        }

        public static void SaveXMLDoc(XmlDocument xmlDoc, string saveFilePath)
        {
            xmlDoc.Save(saveFilePath);
        }

        public static XmlDocument LoadXMLFile(string saveFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(saveFilePath);
            return xmlDoc;
        }

        public static string GetXMLNodeInnerText(XmlDocument xmlDoc, string xpath)
        {
            xpath = Regex.Replace(xpath, "/", "//");
            XmlNode targetNode = (XmlNode)xmlDoc.SelectSingleNode(xpath);
            return targetNode.InnerText;
        }

        public static void SetXMLNodeInnerText(XmlDocument xmlDoc, string xpath, string value)
        {
            xpath = Regex.Replace(xpath, "/", "//");
            XmlNode targetNode = (XmlNode)xmlDoc.SelectSingleNode(xpath);
            targetNode.InnerText = value;
        }
        public static void AddInfo(XmlDocument xmlDoc)
        {
            XmlNode parentNode = xmlDoc.SelectSingleNode("Configuration");
            AppendXMLNode(xmlDoc, parentNode, "PortType", "");
            AppendXMLNode(xmlDoc, parentNode, "COMPort", "");
            AppendXMLNode(xmlDoc, parentNode, "BoudRate", "");
            AppendXMLNode(xmlDoc, parentNode, "CRC", "");
            AppendXMLNode(xmlDoc, parentNode, "AppAddress", "");
            AppendXMLNode(xmlDoc, parentNode, "I2CAddress", "");
            AppendXMLNode(xmlDoc, parentNode, "AppDownloadFile", "");

            parentNode = xmlDoc.SelectSingleNode(@"//PortType");
            AppendXMLNode(xmlDoc, parentNode, "PortType", "USB");
            parentNode = xmlDoc.SelectSingleNode(@"//COMPort");
            AppendXMLNode(xmlDoc, parentNode, "COMPort", "");
            parentNode = xmlDoc.SelectSingleNode(@"//BoudRate");
            AppendXMLNode(xmlDoc, parentNode, "BoudRate", "115200");
            parentNode = xmlDoc.SelectSingleNode(@"//CRC");
            AppendXMLNode(xmlDoc, parentNode, "CRC", "false");
            parentNode = xmlDoc.SelectSingleNode(@"//AppAddress");
            AppendXMLNode(xmlDoc, parentNode, "AppAddress", "08010000");
            parentNode = xmlDoc.SelectSingleNode(@"//I2CAddress");
            AppendXMLNode(xmlDoc, parentNode, "I2CAddress", "A0");
            parentNode = xmlDoc.SelectSingleNode(@"//AppDownloadFile");
            AppendXMLNode(xmlDoc, parentNode, "AppDownloadFile", "");
        }
        public static void LoadValue(XmlDocument xmlDoc)
        {
            ShareParameter.PortType = GetXMLNodeInnerText(xmlDoc, @"/PortType");
            ShareParameter.COMPort = GetXMLNodeInnerText(xmlDoc, @"/COMPort");
            ShareParameter.BoudRate = GetXMLNodeInnerText(xmlDoc, @"/BoudRate");
            ShareParameter.CRC = GetXMLNodeInnerText(xmlDoc, @"/CRC");
            ShareParameter.AppAddress = GetXMLNodeInnerText(xmlDoc, @"/AppAddress");
            ShareParameter.I2CAddress = GetXMLNodeInnerText(xmlDoc, @"/I2CAddress");
            ShareParameter.AppDownloadFile = GetXMLNodeInnerText(xmlDoc, @"/AppDownloadFile");
        }
  
    }
}
