using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CarParser
{
    public class CarModificationDescriptionData
    {
        string ModificationId { get; }
        string Type { get; }
        string EngineCode { get; }
        string FuelType { get; }
        string Volume { get; }
        string Power { get; }
        string ModelYear { get; }

        public CarModificationDescriptionData(string modificationId, string type, string engineCode, string fuelType, string volume, string power, string modelYear)
        {
            ModificationId = modificationId;
            Type = type;
            EngineCode = engineCode;
            FuelType = fuelType;
            Volume = volume;
            Power = power;
            ModelYear = modelYear;
        }

        public void GetCarModificationData(ref string modificationId, ref string modificationType, ref string modificationEngineCode, ref string modificationFuelType, ref string modificationVolume, ref string modificationPower, ref string modificationModelYear)
        {
            modificationId = ModificationId;
            modificationType = Type;
            modificationEngineCode = EngineCode;
            modificationFuelType = FuelType;
            modificationVolume = Volume;
            modificationPower = Power;
            modificationModelYear = ModelYear;
        }
    }
}