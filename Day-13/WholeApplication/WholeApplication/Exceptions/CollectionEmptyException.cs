﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholeApplication.Exceptions
{
    public class CollectionEmptyException : Exception
    {
        private string _message = "Collection is empty";
        public CollectionEmptyException(string msg)
        {
            _message = msg;
        }
        public override string Message => _message;
    }
}