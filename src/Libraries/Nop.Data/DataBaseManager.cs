using Nop.Core.Data;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nop.Data
{
    /// <summary>
    /// Represents the database manager
    /// </summary>
    public partial class DataBaseManager
    {
        private static IDataProvider _dataProvider;
        private static IDbContextOptionsBuilderHelper _dbContextOptionsBuilderHelper;
        private static Assembly _dataProviderAssembly;

        private static int cout;

        static DataBaseManager()
        {
            cout = 0;
        }

        /// <summary>
        /// Current database provider
        /// </summary>
        public static IDataProvider DataProvider {
            get {
                return _dataProvider;
            }
            set {
                _dataProvider = value;
                var finder = new WebAppTypeFinder();
                var assemb = new List<Assembly>() { _dataProvider.GetType().Assembly };

                var type = finder.FindClassesOfType<IDbContextOptionsBuilderHelper>(assemb).First();
                _dbContextOptionsBuilderHelper = (IDbContextOptionsBuilderHelper)Activator.CreateInstance(type);
                _dataProviderAssembly = _dataProvider.GetType().Assembly;
                cout += 1;
            }

        }

        /// <summary>
        /// Current database context options builder helper
        /// </summary>
        public static IDbContextOptionsBuilderHelper DbContextOptionsBuilderHelper {
            get { return _dbContextOptionsBuilderHelper; }
        }

        /// <summary>
        /// Current database provider assembly
        /// </summary>
        public static Assembly DataProviderAssembly
        {
            get { return _dataProviderAssembly; }
        }

        public static int Count
        {
            get { return cout; }
        }


    }
}
