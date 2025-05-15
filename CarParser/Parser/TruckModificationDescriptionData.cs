using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CarParser
{
    public class TruckModificationDescriptionData
    {
        string ModificationId { get; }
        string Type { get; }
        string EngineCode { get; }
        string Volume { get; }
        string Bridge { get; }
        string Power { get; }
        string ModelYear { get; }
        string ManufactureType { get; }
        string Weight { get; }

        public TruckModificationDescriptionData(string modificationId, string group, string type, string engineCode, string volume, string bridge, string power, string modelYear, string manufactureType, string weight)
        {
            ModificationId = modificationId;
            Type = type;
            EngineCode = engineCode;
            Volume = volume;
            Bridge = bridge;
            Power = power;
            ModelYear = modelYear;
            ManufactureType = manufactureType;
            Weight = weight;
        }

        public void GetTruckModificationData(ref string modificationId, ref string modificationType, ref string modificationEngineCode, ref string modificationVolume, ref string modificationBridge, ref string modificationPower, ref string modificationModelYear, ref string modificationManufactureType, ref string modificationWeight)
        {
            modificationId = ModificationId;
            modificationType = Type;
            modificationEngineCode = EngineCode;
            modificationVolume = Volume;
            modificationBridge = Bridge;
            modificationPower = Power;
            modificationModelYear = ModelYear;
            modificationManufactureType = ManufactureType;
            modificationWeight = Weight;
        }
    }
}