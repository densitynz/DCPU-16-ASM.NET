﻿namespace DCPU16_ASM
{
    public class OpParamResult
    {
        public OpParamResult()
        {
            this.Param = 0x0;
            this.NextWord = false;
            this.NextWordValue = 0x0;
            this.LabelName = string.Empty;
            this.Illegal = false;
        }

        public ushort Param { get; set; }

        public bool NextWord { get; set; }

        public ushort NextWordValue { get; set; }

        public string LabelName { get; set; }

        public bool Illegal { get; set; }
    }
}