﻿using System.Collections.Generic;
using Digipost.Api.Client.Tests.CompareObjects;
using Xunit;

namespace Digipost.Api.Client.DataTypes.Tests.Appointment
{
    public class InfoTests
    {
        private static readonly Comparator Comparator = new Comparator();

        [Fact]
        public void AsDataTransferObject()
        {
            var source = new Info("Title", "Text");
            var expected = source.AsDataTransferObject();

            var actual = new info()
            {
                title = "Title",
                text = "Text",
            };

            IEnumerable<IDifference> differences;
            Comparator.Equal(expected, actual, out differences);
            Assert.Empty(differences);
        }

    }
}
