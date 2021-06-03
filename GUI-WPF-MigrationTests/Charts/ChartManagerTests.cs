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
            ChartManager chartManager = new ChartManager(null);

            PrivateObject chartManagerPO = new PrivateObject(chartManager);

            Assert.IsNotNull(chartManagerPO.GetField("modules"), "The Modules Dictionary of ChartManager was found null.");
        }

        [TestMethod()]
        public void ParseReceivedData_IncorrectFormat_ShouldReturnNull()
        {
            ChartManager chartManager = new ChartManager(null);
            string wrongString = "CONFIG_HEADERxDATA";

            var result = chartManager.ParseReceivedData(wrongString);

            Assert.IsFalse(result.HasValue);
        }

        [TestMethod()]
        public void ParseReceivedData_CorrectFormat_ShouldReturnHeaderAndData()
        {
            ChartManager chartManager = new ChartManager(null);
            string wrongString = "CONFIG_HEADER|DATA";

            var result = chartManager.ParseReceivedData(wrongString);

            Assert.IsTrue(result.HasValue);
        }

        [TestMethod()]
        public void HostPipeServer_PipeStream_ShouldBeCreated()
        {
            ChartManager chartManager = new ChartManager(null);

            chartManager.HostPipeServer();

            PrivateObject chartManagerPO = new PrivateObject(chartManager);

            Assert.IsNotNull(chartManagerPO.GetField("pipeStream"));

            Console.WriteLine("Disposing...");

            chartManager.Dispose();

            Console.WriteLine("Finished disposing");
        }

        [TestMethod()]
        public void HostPipeServer_Status_ShouldBeLoading()
        {
            ChartManager chartManager = new ChartManager(null);

            Console.WriteLine("Starting hosting...");

            chartManager.HostPipeServer();

            PrivateObject chartManagerPO = new PrivateObject(chartManager);

            Assert.AreEqual((ChartManager.Status)chartManagerPO.GetField("status"), ChartManager.Status.Loading);

            chartManager.Dispose();
        }
    }
}