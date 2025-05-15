using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CarParser
{
    public class TrailerModificationDescriptionData
    {
        string ModificationId { get; }
        string Group { get; }
        string Type { get; }
        string MakeName { get; }
        string Code { get; }
        string Description { get; }

        public TrailerModificationDescriptionData(string modificationId, string group, string type, string makeName, string code, string description)
        {
            ModificationId = modificationId;
            Group = group;
            Type = type;
            MakeName = makeName;
            Code = code;
            Description = description;
        }

        public void GetTrailerModificationData(ref string modificationId, ref string modificationGroup, ref string modificationType, ref string modificationMakeName, ref string modificationCode, ref string modificationDescription)
        {
            modificationId = ModificationId;
            modificationGroup = Group;
            modificationType = Type;
            modificationMakeName = MakeName;
            modificationCode = Code;
            modificationDescription = Description;
        }
    }
}