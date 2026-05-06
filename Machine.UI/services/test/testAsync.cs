using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.UI.services.test
{
    public class testAsync
    {

        async Task<string> GetDataAsync()
        {
             // Simulate an asynchronous operation
             await Task.Delay(1000);
            return "string!";
        }
        async Task<string> GetData() {

            string data = null;

            string x = data+ await GetDataAsync();
            Task task = Task.Run(async () =>
            {
                await GetDataAsync();
                await Task.Run(() => runT1());
                ;
            });
            await task;
            Task<string> t;
            return data;
        }
        public void runT1() { }
        public void runT2() { }


    }
}
