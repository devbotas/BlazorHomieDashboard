﻿using System.Collections.Generic;

namespace Bhd.Shared.DTOs {
    public class Property {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public Direction Direction { get; set; }
        public double NumericValue { get; set; }
        public string TextValue { get; set; }
        public string Format { get; set; }
        public string Unit { get; set; }
        public List<string> Choices { get; set; } = new List<string>();
    }
}