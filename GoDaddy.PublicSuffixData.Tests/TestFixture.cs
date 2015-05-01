using System;
using System.Collections.Generic;
using Moq;

namespace GoDaddy.PublicSuffixData.Tests
{
    public abstract class TestFixture
    {
        private readonly IDictionary<Type, Mock> _mockCache = new Dictionary<Type, Mock>();

        public Mock<TDependency> Mocked<TDependency>() where TDependency : class
        {
            var type = typeof (TDependency);
            if (!_mockCache.ContainsKey(type))
            {
                _mockCache[type] = new Mock<TDependency>();
            }

            return (Mock<TDependency>)_mockCache[typeof(TDependency)];
        }
    }
}
