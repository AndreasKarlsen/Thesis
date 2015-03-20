﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.Lockbased
{
    public class TMInt : TMVar<int>
    {

        public TMInt() : base()
        {

        }

        public TMInt(int i) : base(i)
        {

        }

        #region Inc Dec
        public void Inc()
        {
            this.Value = this.Value + 1;
        }

        public void Dec()
        {
            this.Value = this.Value -1;
        }

        public static TMInt operator ++(TMInt tmint)
        {
            tmint.Inc();
            return tmint;
        }

        public static TMInt operator --(TMInt tmint)
        {
            tmint.Dec();
            return tmint;
        }

        #endregion Inc Dec
        /*
        #region Binary
        public static int operator +(TMInt tmint, int i)
        {
            return tmint.Value + i;
        }

        public static int operator -(TMInt tmint, int i)
        {
            return tmint.Value - i;
        }

        public static int operator *(TMInt tmint, int i)
        {
            return tmint.Value * i;
        }

        public static int operator /(TMInt tmint, int i)
        {
            return tmint.Value / i;
        }

        public static int operator %(TMInt tmint, int i)
        {
            return tmint.Value % i;
        }

        #endregion Binary
        //==, !=, <, >, <=, >=

        #region BinaryBool

        public static bool operator <(TMInt tmint, int i)
        {
            return tmint.Value < i;
        }

        public static bool operator >(TMInt tmint, int i)
        {
            return tmint.Value > i;
        }

        public static bool operator >=(TMInt tmint, int i)
        {
            return tmint.Value >= i;
        }

        public static bool operator <=(TMInt tmint, int i)
        {
            return tmint.Value <= i;
        }

        #endregion BinaryBool
        */
    }
}