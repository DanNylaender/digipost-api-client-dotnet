﻿using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects;

namespace Digipost.Api.Client.Test.CompareObjects
{
    internal class Comparator : IComparator
    {
        public bool Equal(object expected, object actual)
        {
            IEnumerable<IDifference> differences;
            return Equal(expected, actual, out differences);
        }

        public ComparisonConfiguration ComparisonConfiguration { get; set; } = new ComparisonConfiguration();

        public bool Equal(object expected, object actual, out IEnumerable<IDifference> differences)
        {
            var compareLogic = new CompareLogic(
                new ComparisonConfig
                {
                    MaxDifferences = 5,
                    IgnoreObjectTypes = true
                });
            

            var compareResult = compareLogic.Compare(expected, actual);

            differences = compareResult.Differences.Select(d => new Difference
            {
                PropertyName = d.PropertyName,
                WhatIsCompared = d.GetWhatIsCompared(),
                ExpectedValue = d.Object1Value,
                ActualValue = d.Object2Value
            }).ToList<IDifference>();

            return compareResult.AreEqual;
        }
    }
}