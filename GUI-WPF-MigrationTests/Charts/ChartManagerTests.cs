using Microsoft.VisualStudio.TestTools.UnitTesting;
using Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charts.Tests
{
    [TestClass()]
    public class ChartManagerTests
    {
        [TestMethod()]
        public void Constructor_ModulesDictionary_ShouldInitialize()
        {
            var chartManager = new ChartManager(null);

            var chartManagerPo = new PrivateObject(chartManager);

            Assert.IsNotNull(chartManagerPo.GetField("modules"), "The Modules Dictionary of ChartManager was found null.");
        }

        [TestMethod()]
        public void ParseReceivedData_IncorrectFormat_ShouldReturnNull()
        {
            var chartManager = new ChartManager(null);
            const string wrongString = "CONFIG_HEADERxDATA";

            var result = chartManager.ParseReceivedData(wrongString);

            Assert.IsFalse(result.HasValue);
        }

        [TestMethod()]
        public void ParseReceivedData_CorrectFormat_ShouldReturnHeaderAndData()
        {
            var chartManager = new ChartManager(null);
            const string wrongString = "CONFIG_HEADER|DATA";

            var result = chartManager.ParseReceivedData(wrongString);

            Assert.IsTrue(result.HasValue);
        }

        [TestMethod()]
        public void HostPipeServer_PipeStream_ShouldBeCreated()
        {
            var chartManager = new ChartManager(null);

            chartManager.HostPipeServer();

            var chartManagerPo = new PrivateObject(chartManager);

            Assert.IsNotNull(chartManagerPo.GetField("pipeStream"));

            Console.WriteLine("Disposing...");

            chartManager.Dispose();

            Console.WriteLine("Finished disposing");
        }

        [TestMethod()]
        public void HostPipeServer_Status_ShouldBeLoading()
        {
            var chartManager = new ChartManager(null);

            Console.WriteLine("Starting hosting...");

            chartManager.HostPipeServer();

            var chartManagerPo = new PrivateObject(chartManager);

            Assert.AreEqual((ChartManager.CmStatus)chartManagerPo.GetField("Status"), ChartManager.CmStatus.Loading);

            chartManager.Dispose();
        }
    }
}