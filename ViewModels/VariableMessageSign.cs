﻿namespace Matrix.ViewModels
{
    public class VariableMessageSign
    {
        public string Id { get; set; }

        public string Sign { get; set; }

        public override string ToString()
        {
            return Sign;
        }
    }
}
