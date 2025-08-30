using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HaynesProParser.Managers.TransportManagers
{
    public static class TransportModelManager
    {
        public static List<object> GetRowsInfo((ChromeDriver, WebDriverWait) modelDriverPool, bool isTruck = false)
        {
            List<object> rowValues = [];

            IWebElement table = modelDriverPool.Item1.FindElement(By.TagName("tbody"));

            List<IWebElement> rows = table.FindElements(By.TagName("tr")).ToList();

            if (rows.Count == 0)
            {
                return rowValues;
            }

            rows.RemoveAt(0);

            foreach (IWebElement row in rows)
            {
                string modificationId = row.GetAttribute("data-typeid");
                string fuelTypeOrIsDisabled = row.GetAttribute("class");

                ReadOnlyCollection<IWebElement> tableDatas = row.FindElements(By.TagName("td"));

                if (!isTruck)
                {
                    // Легковой автомобиль
                    string modificationType = tableDatas[0].Text?.Trim();
                    string engineCode = tableDatas[1].Text?.Trim();
                    string capacity = tableDatas[2].Text?.Trim();
                    string power = tableDatas[3].Text?.Trim();
                    string modelYears = tableDatas[4].Text?.Trim();

                    rowValues.Add(new CarTempRowData(
                        modificationId,
                        modificationType,
                        engineCode,
                        capacity,
                        power,
                        modelYears,
                        fuelTypeOrIsDisabled
                    ));
                }
                else
                {
                    // Грузовик
                    bool isDisabled = fuelTypeOrIsDisabled.Contains("disabled");
                    string modificationType = tableDatas[0].Text?.Trim();
                    string axle = tableDatas[1].Text?.Trim();
                    string engineCode = tableDatas[2].Text?.Trim();
                    string capacity = tableDatas[3].Text?.Trim();
                    string power = tableDatas[4].Text?.Trim();
                    string modelYears = tableDatas[5].Text?.Trim();
                    string buildType = tableDatas[6].Text?.Trim();
                    string tonnage = tableDatas[7].Text?.Trim();

                    rowValues.Add(new TruckTempRowData(
                        modificationId,
                        modificationType,
                        axle,
                        engineCode,
                        capacity,
                        power,
                        modelYears,
                        buildType,
                        tonnage,
                        isDisabled
                    ));
                }
            }

            return rowValues;
        }
    }

    public class CarTempRowData
    {
        public string ModId { get; set; }
        public string ModType { get; set; }
        public string EngineCode { get; set; }
        public string Capacity { get; set; }
        public string Power { get; set; }
        public string ModelYears { get; set; }
        public string FuelType { get; set; }

        public CarTempRowData(string modId, string modType, string engineCode, string capacity, string power, string modelYears, string fuelType)
        {
            ModId = modId;
            ModType = modType;
            EngineCode = engineCode;
            Capacity = capacity;
            Power = power;
            ModelYears = modelYears;
            FuelType = fuelType;
        }
    }

    public class TruckTempRowData
    {
        public string ModId { get; set; }
        public string ModType { get; set; }
        public string Axle { get; set; }
        public string EngineCode { get; set; }
        public string Capacity { get; set; }
        public string Power { get; set; }
        public string ModelYears { get; set; }
        public string BuildType { get; set; }
        public string Tonnage { get; set; }
        public bool IsDisabled { get; set; }

        public TruckTempRowData(string modId, string modType, string axle, string engineCode, string capacity, string power, string modelYears, string buildType, string tonnage, bool isDisabled)
        {
            ModId = modId;
            ModType = modType;
            Axle = axle;
            EngineCode = engineCode;
            Capacity = capacity;
            Power = power;
            ModelYears = modelYears;
            BuildType = buildType;
            Tonnage = tonnage;
            IsDisabled = isDisabled;
        }
    }
}
