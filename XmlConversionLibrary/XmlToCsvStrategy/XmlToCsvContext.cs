using System.Text;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvContext
    {
        private readonly XmlToCsvStrategyBase _strategy;

        public XmlToCsvStrategyBase Strategy
        {
            get { return _strategy; }
        } 

        public XmlToCsvContext(XmlToCsvStrategyBase strategy)
        {
            _strategy = strategy;
        }

        public void Execute(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            _strategy.ExportToCsv(xmlTableName, csvDestinationFilePath, encoding);
        }
    }
}