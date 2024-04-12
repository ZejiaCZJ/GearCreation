﻿using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace GearCreation
{
    public class GearCreationInfo : GH_AssemblyInfo
    {
        public override string Name => "GearCreation";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("6113988d-2d5c-4eed-8f43-4d7969688a57");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}